// NeuroC_ComVision.cpp : Hiermit werden die exportierten Funktionen für die DLL definiert.
//

#include "pch.h"
#include "NeuroC_ComVision.h"

#include <opencv2/opencv.hpp>
#include <thread>
#include <atomic>
#include <mutex>

static cv::VideoCapture cap;
static std::atomic<bool> running(false);
static cv::Mat currentFrame;
static std::mutex frameMutex;

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

bool GetFrame(DetectionResult* result)
{
    std::lock_guard<std::mutex> lock(frameMutex);

    if (currentFrame.empty())
        return false;

    cv::Mat hsv;
    cv::cvtColor(currentFrame, hsv, cv::COLOR_BGR2HSV);

    cv::Mat mask;

    cv::inRange(hsv,
        cv::Scalar(0, 120, 70),
        cv::Scalar(10, 255, 255),
        mask);

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

void StopCamera()
{
    running = false;
    cap.release();
}
