# FAQ Chatbot (C# / ASP.NET Core / Azure)

## A lightweight and customizable FAQ chatbot built with ASP.NET Core. It uses keyword matching and cosine similarity to return accurate answers and includes a modern chat interface.

## What This Project Does

## This project implements a smart FAQ chatbot that:

+ Answers common questions using keyword scoring and cosine similarity
+ Handles greetings, farewells, and casual conversation
+ Maintains light conversation context (yes/no confirmations)
+ Includes a clean web chat UI with animations and quick-question buttons
+ Runs without the Azure Bot Framework or any authentication requirements

## Tech Stack

+ Backend: ASP.NET Core (C#)
+ Frontend: HTML, CSS, JavaScript
+ Hosting: Azure App Service
+ Storage: JSON file (FAQs.json) for FAQ data

## Core Bot Logic (FAQBot.cs)

## The bot processes user messages through several stages:

+ Checks conversation context (simple yes/no flow)
+ Detects greetings and casual phrases
+ Runs multi-layered matching:
+ Keyword scoring
+ Cosine similarity
+ Single-word fallback
+ Generates “Did you mean…?” suggestions
+ Uses a default fallback message when nothing matches
+ FAQs are stored in FAQs.json for easy editing.

## API Layer (ChatController.cs)

## The backend exposes a single POST endpoint:

+ POST /api/chat
+ Accepts messages and optional conversation IDs
+ Processes messages through FAQBot
+ Returns structured JSON responses
+ Includes basic error handling
+ Frontend (index.html)
+ A clean, responsive chat interface featuring:
+ Gradient purple UI
+ Animated message bubbles
+ Typing indicator
+ Quick-suggestion buttons
+ Works on desktop and mobile
  
## Project Configuration

## Startup.cs configured for:

+ Static file hosting

+ API routing

+ Registering FAQBot as a singleton

+ wwwroot holds all frontend assets

+ FAQs.json is set to copy to the output directory

## Deployment to Azure

+ The project is deployed via Azure App Service:
+ Published directly from Visual Studio
+ Static files served from the root URL
+ API available at /api/chat
+ No Microsoft Entra ID configuration required
+ No Bot Framework or OAuth

## Challenges and Solutions

+ Originally, the bot was built with Azure Bot Service and the Bot Framework SDK. This required bot registration, Entra ID app configuration, and OAuth credentials, which led to authentication errors where the bot could receive messages but not send replies.

## Resolution:

+ Removed all Bot Framework dependencies
+ Rebuilt as a simple REST API + custom chat UI
+ Reduced complexity and improved portability
+ Simplified deployment significantly

## Architecture
User Browser
    ↓
index.html (Chat UI)
    ↓
ChatController (REST API)
    ↓
FAQBot (Core Logic)
    ↓
Response

## Local Development

+ Clone the repository

+ Open in Visual Studio

+ Run the project

+ Navigate to: https://localhost:5000, no authentication required.
