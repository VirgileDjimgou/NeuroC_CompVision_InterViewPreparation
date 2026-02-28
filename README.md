\# NeuroC\_ComVision



\## Brainstorming Playground - Interview Preparation Edition 😄



This project is NOT a revolutionary industrial vision framework. It

is simply a technical brainstorming playground I built while preparing

for a job interview at NeuroCheck.



Nothing overly ambitious. No hidden AI breakthrough. No secret GPU

cluster.



Just structured thinking, architectural experiments, and hands‑on

technical preparation.



------------------------------------------------------------------------



\## Purpose



The goal of this project was to:



\-   Practice C++ / OpenCV integration

\-   Design a clean DLL export interface

\-   Connect native C++ code with a C# WPF application

\-   Simulate a typical industrial vision software architecture

\-   Refresh multithreading and interop concepts before an interview



In short:



> A focused technical rehearsal --- nothing more, nothing less.



------------------------------------------------------------------------



\## Architecture Used



Laptop Camera\\

↓\\

C++ OpenCV Processing Engine (DLL)\\

↓\\

C-compatible Export Interface\\

↓\\

C# WPF Client via P/Invoke\\

↓\\

Simple UI displaying object position



------------------------------------------------------------------------



\## Technologies



\-   C++17

\-   OpenCV 4.x

\-   Windows DLL (native exports)

\-   Multithreading (std::thread, std::mutex)

\-   C# (.NET)

\-   WPF

\-   P/Invoke



------------------------------------------------------------------------



\##  What It Does



\-   Captures frames from the laptop camera

\-   Detects a red object using HSV filtering

\-   Computes a bounding box

\-   Displays X/Y coordinates in a WPF interface



That's it.



No machine learning. No deep learning. No enterprise scalability layer.

Just clean fundamentals.



------------------------------------------------------------------------



\## Why It Exists



Interview preparation should be practical.



Instead of only reviewing theory, I prefer to:



\-   Implement small, focused prototypes

\-   Revisit core architectural principles

\-   Validate cross-language integration

\-   Think through real-world design patterns



This repository reflects that mindset.



------------------------------------------------------------------------



\## ⚠ Disclaimer



This project is intentionally simple.



It is a preparation exercise, not a production system. Please do not

benchmark it against industrial-grade vision platforms 🙂



------------------------------------------------------------------------



\## 👤 Author



Patrick Djimgou\\

Germany\\

Interview preparation session -- NeuroCheck



------------------------------------------------------------------------



If you are reading this and expected something world-changing:



Sorry 😄\\

But fundamentals always win.



