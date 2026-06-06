<img width="1072" height="783" alt="Animation" src="https://github.com/user-attachments/assets/adb7ef20-1cab-4295-9c2b-09dac8798dcc" /><img width="1072" height="783" alt="Animation" src="https://github.com/user-attachments/assets/e22aeabc-ff37-41aa-8167-5f1338f7d71b" /># 🧭 A* Pathfinding Algorithm Simulator

An interactive, highly visual desktop application that simulates the **A* (A-Star) Pathfinding Algorithm**. Built entirely in C#, this project demonstrates core computer science algorithms, event-driven programming, and high-performance UI rendering.

![A-Star Simulation Demo]
[cv_normal.odt](https://github.com/user-attachments/files/28673450/cv_normal.odt)


## ✨ Features
* **Interactive Grid:** Draw and erase walls in real-time using smooth mouse drag interactions.
* **Algorithm Visualization:** Watch the A* algorithm "think"! The simulation beautifully animates the exploration of nodes (open and closed sets) and the final shortest-path discovery.
* **Zero UI Freezes:** Utilizes modern `async/await` (Task-based Asynchronous Pattern) to ensure the application remains completely responsive during complex pathfinding calculations.
* **High-Performance Rendering:** Instead of using thousands of heavy UI controls, the entire grid is painted natively using the **GDI+** engine (`System.Drawing`) for maximum memory efficiency and zero lag.

## 🛠️ Tech Stack & Key Concepts
* **Language:** C# 8.0+
* **Framework:** .NET (Windows Forms)
* **Rendering:** GDI+ Graphics API
* **Core Skills Demonstrated:** * Graph Theory & Search Algorithms
  * Heuristic Distance Calculations (Manhattan Distance)
  * Asynchronous Programming
  * Object-Oriented Design (OOP) & Clean State Management

## 🚀 How to Use
1. Clone this repository to your local machine.
2. Open the `.sln` file in Visual Studio.
3. Hit `F5` to build and run the project.
4. **Left Click & Drag:** Draw obstacles (walls).
5. **Right Click & Drag:** Erase obstacles.
6. Click the **Find Path** button to start the simulation!

## 👨‍💻 About
Developed by **Volkan Çıbık** *Computer Programming, Eskişehir Teknik Üniversitesi (Class of 2026)*

This project was built as part of my portfolio to visualize low-level algorithmic concepts, improve memory management, and demonstrate rendering optimization skills in C#.
