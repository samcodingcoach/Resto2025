# Resto2025 - Restaurant Management System

## Overview
Resto2025 is a cross-platform restaurant management application built with .NET MAUI. It provides comprehensive functionality for restaurant operations including ordering, payment processing, table management, and customer management.

## Platform Support
- Android
- iOS
- macOS Catalyst
- Windows

## Core Features

### 1. Product Menu Management
- Displays products with images, prices, and stock levels
- Categorized menu display
- Search functionality for menu items
- Real-time stock tracking
- Product filtering by category

### 2. Customer Management
- Guest and member ordering options
- Customer registration system
- Customer search by phone number
- Member management with contact details

### 3. Order Types
- Takeaway orders
- Dine-in orders with table selection
- Interactive table layout with zoom functionality
- Real-time table status (occupied/available)

### 4. Shopping Cart
- Add products to cart from menu
- Edit item quantities
- Split items between takeaway and dine-in
- Real-time cart calculations

### 5. Payment Processing
- Multiple payment methods:
  - Cash (with calculator interface)
  - Bank transfer
  - QRIS (QR Code payment)
- Invoice system for later payments
- Payment confirmation and receipt handling

### 6. Order Tracking
- View existing orders for tables
- Order details with item breakdown
- Payment status tracking
- Table release functionality after payment

## Technical Architecture

### Backend Integration
- API endpoints hosted at https://resto.samdev.org/_resto007/api/
- Image hosting at https://resto.samdev.org/_resto007/public/images/
- RESTful API communication using JSON

### Data Models
- Products (with stock, pricing, images)
- Categories
- Customers
- Tables
- Orders
- Payments
- Promotions

### UI Components
- Navigation flow with tab-based interface
- Popup modals for payment processing
- Interactive table layout
- Real-time cart summary
- Payment calculator with keypad

### Key Classes

#### ProdukMenu.xaml.cs
- Main ordering interface with 4 main sections:
  - Customer selection
  - Order mode (takeaway/dine-in)
  - Product menu with categories
  - Payment methods
- Handles cart management and calculations
- Processes payments and saves orders

#### CekPesanan_Modal.xaml.cs
- Modal for viewing existing orders for a table
- Shows order details including items, pricing, and status
- Allows payment processing or table release

#### Payment Modals
- Tunai_Modal: Cash payment with calculator
- TransferBank_Modal: Bank transfer details
- Qris_Modal: QRIS payment processing

### Dependencies
- CommunityToolkit.Maui
- Microsoft.Maui.Controls
- Newtonsoft.Json
- SkiaSharp

## Business Logic

### Pricing Calculations
- Subtotal: Sum of all products
- Takeaway fees: Applied per takeaway item
- Service charges: Based on payment method (especially QRIS)
- Promotions: Percentage or fixed amount discounts
- Tax (PPN): Applied to subtotal
- Final total rounded to nearest 100

### Order Flow
1. Customer selection (Guest or Member)
2. Order mode selection (Takeaway or Dine-in)
3. Table selection (for dine-in orders)
4. Product selection and cart building
5. Payment method selection
6. Payment processing
7. Order confirmation

### Data Persistence
- Temporary order storage using local files
- Real-time synchronization with backend
- Session management for user orders

## UI/UX Features
- Modern, restaurant-themed interface
- Green color scheme (#075E54) with complementary colors
- Responsive layout with two-column design
- Intuitive cart management
- Visual indicators for stock availability
- Interactive elements with animations