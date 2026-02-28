// NeuroC_ComVision.cpp : Hiermit werden die exportierten Funktionen für die DLL definiert.
//

#include "pch.h"
#include "NeuroC_ComVision.h"

#include <opencv2/opencv.hpp>
#include <opencv2/objdetect.hpp>
#include <thread>
#include <atomic>
#include <mutex>

// ========== Globaler Zustand ==========
static cv::VideoCapture cap;
static std::atomic<bool> running(false);
static cv::Mat currentFrame;
static std::mutex frameMutex;
static cv::CascadeClassifier faceCascade;

// ========== Kamera-Steuerung ==========

bool StartCamera()
{
    cap.open(0);
    if (!cap.isOpened())
        return false;

    running = true;

    std::thread([]()
    {
        while (running)
        {
            cv::Mat frame;
            cap >> frame;
            if (!frame.empty())
            {
                std::lock_guard<std::mutex> lock(frameMutex);
                currentFrame = frame.clone();
            }
        }
    }).detach();

    return true;
}

void StopCamera()
{
    running = false;
    if (cap.isOpened())
        cap.release();
}

// ========== Bestehende Farberkennung ==========

bool GetFrame(DetectionResult* result)
{
    std::lock_guard<std::mutex> lock(frameMutex);
    if (currentFrame.empty())
        return false;

    cv::Mat hsv;
    cv::cvtColor(currentFrame, hsv, cv::COLOR_BGR2HSV);

    cv::Mat mask;
    cv::inRange(hsv, cv::Scalar(0, 120, 70), cv::Scalar(10, 255, 255), mask);

    std::vector<std::vector<cv::Point>> contours;
    cv::findContours(mask, contours, cv::RETR_EXTERNAL, cv::CHAIN_APPROX_SIMPLE);

    if (!contours.empty())
    {
        auto rect = cv::boundingRect(contours[0]);
        result->x = rect.x;
        result->y = rect.y;
        result->width = rect.width;
        result->height = rect.height;
        result->detected = true;
    }
    else
    {
        result->detected = false;
    }
    return true;
}

// ========== NEU: Frame-Rohdaten ==========

bool GetFrameInfo(FrameInfo* info)
{
    std::lock_guard<std::mutex> lock(frameMutex);
    if (currentFrame.empty())
        return false;

    info->width    = currentFrame.cols;
    info->height   = currentFrame.rows;
    info->channels = currentFrame.channels();
    info->stride   = static_cast<int>(currentFrame.step[0]);
    info->totalBytes = info->stride * info->height;
    return true;
}

// BGR-Rohdaten (nativ für OpenCV)
bool GetFrameBytes(unsigned char* buffer, int bufferSize)
{
    std::lock_guard<std::mutex> lock(frameMutex);
    if (currentFrame.empty())
        return false;

    int needed = static_cast<int>(currentFrame.step[0]) * currentFrame.rows;
    if (bufferSize < needed)
        return false;

    memcpy(buffer, currentFrame.data, needed);
    return true;
}

// RGB-Rohdaten (für WPF BitmapSource Rgb24)
bool GetFrameBytesRgb(unsigned char* buffer, int bufferSize)
{
    std::lock_guard<std::mutex> lock(frameMutex);
    if (currentFrame.empty())
        return false;

    cv::Mat rgb;
    cv::cvtColor(currentFrame, rgb, cv::COLOR_BGR2RGB);

    // Fortlaufende Bytes sicherstellen
    if (!rgb.isContinuous())
        rgb = rgb.clone();

    int needed = rgb.cols * rgb.rows * rgb.channels();
    if (bufferSize < needed)
        return false;

    memcpy(buffer, rgb.data, needed);
    return true;
}

// ========== NEU: Gesichtserkennung ==========

bool LoadFaceCascade(const char* cascadePath)
{
    return faceCascade.load(cascadePath);
}

bool DetectFaces(MultiDetectionResult* result)
{
    std::lock_guard<std::mutex> lock(frameMutex);
    if (currentFrame.empty() || faceCascade.empty())
        return false;

    cv::Mat gray;
    cv::cvtColor(currentFrame, gray, cv::COLOR_BGR2GRAY);
    cv::equalizeHist(gray, gray);

    std::vector<cv::Rect> faces;
    faceCascade.detectMultiScale(gray, faces, 1.1, 5,
        cv::CASCADE_SCALE_IMAGE, cv::Size(30, 30));

    result->count = 0;
    for (size_t i = 0; i < faces.size() && i < 32; i++)
    {
        result->items[i].x       = faces[i].x;
        result->items[i].y       = faces[i].y;
        result->items[i].width   = faces[i].width;
        result->items[i].height  = faces[i].height;
        result->items[i].detected = true;
        result->count++;
    }
    return true;
}

// ========== NEU: Kantenerkennung (Canny) ==========

bool DetectEdges(unsigned char* outputBuffer, int bufferSize,
                 int* outWidth, int* outHeight)
{
    std::lock_guard<std::mutex> lock(frameMutex);
    if (currentFrame.empty())
        return false;

    cv::Mat gray, edges;
    cv::cvtColor(currentFrame, gray, cv::COLOR_BGR2GRAY);
    cv::GaussianBlur(gray, gray, cv::Size(5, 5), 1.4);
    cv::Canny(gray, edges, 50, 150);

    *outWidth  = edges.cols;
    *outHeight = edges.rows;

    int needed = edges.cols * edges.rows; // 1 Kanal
    if (bufferSize < needed)
        return false;

    if (!edges.isContinuous())
        edges = edges.clone();

    memcpy(outputBuffer, edges.data, needed);
    return true;
}

// ========== NEU: Kreiserkennung (HoughCircles) ==========

bool DetectCircles(MultiDetectionResult* result)
{
    std::lock_guard<std::mutex> lock(frameMutex);
    if (currentFrame.empty())
        return false;

    cv::Mat gray;
    cv::cvtColor(currentFrame, gray, cv::COLOR_BGR2GRAY);
    cv::GaussianBlur(gray, gray, cv::Size(9, 9), 2);

    std::vector<cv::Vec3f> circles;
    cv::HoughCircles(gray, circles, cv::HOUGH_GRADIENT,
        1,               // dp
        gray.rows / 8,   // minDist
        100,             // param1 (Canny-Schwelle)
        40,              // param2 (Akkumulator-Schwelle)
        20,              // minRadius
        200              // maxRadius
    );

    result->count = 0;
    for (size_t i = 0; i < circles.size() && i < 32; i++)
    {
        int cx = cvRound(circles[i][0]);
        int cy = cvRound(circles[i][1]);
        int r  = cvRound(circles[i][2]);

        // Bounding-Box um den Kreis
        result->items[i].x       = cx - r;
        result->items[i].y       = cy - r;
        result->items[i].width   = r * 2;
        result->items[i].height  = r * 2;
        result->items[i].detected = true;
        result->count++;
    }
    return true;
}
