# Gemini Development Guide

This document provides guidance for developers and Gemini on how to work with the `cloud-native-shop` frontend project.

## Project Overview

The `frontend` is a modern e-commerce web application built with Angular. It provides a user-friendly interface for customers to browse products, manage their shopping cart, and view their orders. The application communicates with a separate backend service for data and business logic.

## Getting Started

### Prerequisites

*   Node.js (v20.19+ or v22.12+)
*   Angular CLI (`npm install -g @angular/cli`)

### Installation

1.  Navigate to the `frontend` directory:
    ```bash
    cd frontend
    ```
2.  Install the dependencies:
    ```bash
    npm install
    ```

### Running the Application

1.  Start the development server:
    ```bash
    ng serve
    ```
2.  Open your browser and navigate to `http://localhost:4200/`.

## Project Structure

The project follows the standard Angular project structure:

```
frontend/
├── src/
│   ├── app/                # Application components, services, and modules
│   │   ├── components/     # Reusable UI components
│   │   ├── pages/          # Page-level components (e.g., ProductListPage)
│   │   ├── services/       # Services for API communication and business logic
│   │   ├── models/         # TypeScript interfaces for data structures
│   │   ├── app.config.ts   # Application configuration
│   │   ├── app.routes.ts   # Application routes
│   │   └── app.component.ts# Root application component
│   ├── assets/             # Static assets (images, fonts, etc.)
│   ├── environments/       # Environment-specific configuration
│   ├── index.html          # Main HTML file
│   ├── main.ts             # Main entry point of the application
│   └── styles.css          # Global styles
├── angular.json            # Angular CLI configuration
├── package.json            # Project dependencies and scripts
└── tsconfig.json           # TypeScript configuration
```

## Development Guidelines

*   **Components:** Create new components under `src/app/components` for reusable UI elements and `src/app/pages` for page-level components.
*   **Services:** Place services that interact with APIs or manage application state in `src/app/services`.
*   **Models:** Define TypeScript interfaces for all data structures in `src/app/models`.
*   **Styling:** Use global styles in `src/styles.css` and component-specific styles in the component's `.css` file.
*   **State Management:** Use RxJS `BehaviorSubject` in services for simple state management. For more complex state, consider using NgRx.
*   **API Integration:** All API calls should be made through services. The base URL for the API should be configured in the `environments` files.
