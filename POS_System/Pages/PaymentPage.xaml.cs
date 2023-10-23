﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using POS_System.Models;

namespace POS_System.Pages
{
    public partial class PaymentPage : Window
    {
        private ObservableCollection<OrderedItem> _orderedItems;
        private string _tableNumber;
        private string _orderType;
        private long _orderId;
        private string _status;
        private bool _hasUnpaidOrders = true;

        String paymentMethod;

        public PaymentPage()
        {
            InitializeComponent();
            InitializeEventHandlers();

        }

        public PaymentPage(ObservableCollection<OrderedItem> orderedItems, string tableNumber, string orderType, long orderId, string status, bool hasUnpaidOrders) : this()
        {
            _tableNumber = tableNumber;
            _orderedItems = orderedItems;
            _orderType = orderType;
            _orderId = orderId;
            _status = status;
            _hasUnpaidOrders = hasUnpaidOrders;

            CultureInfo cultureInfo = new CultureInfo("en-CA");
            cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
            totalAmtTextBox.Text = CalculateTotalOrderAmount().ToString("C", cultureInfo);

            CalculateTaxAmount();
            InitializeEventHandlers();


        }

        private double CalculateTotalOrderAmount()
        {
            double totalAmount = 0;
            foreach (var item in _orderedItems)
            {
                totalAmount += item.ItemPrice;
            }
            return totalAmount;
        }


        private void CancelButton(object sender, RoutedEventArgs e)
        {
            MenuPage menuPage = new MenuPage(_tableNumber, _orderType, _status, true);
            menuPage.Show();
            this.Close();
        }

        

        private void CalculateTaxAmount()
        {
            double totalTaxAmount = 0;
            double totalOrderAmount = CalculateTotalOrderAmount();
            double taxRate = 0.13;
            totalTaxAmount = totalOrderAmount * taxRate;

            CultureInfo cultureInfo = new CultureInfo("en-CA");
            cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
            totalTaxTextBox.Text = totalTaxAmount.ToString("C", cultureInfo);
        }

        // Calculate Order Total Balance and show in the textbox
        private void CalculateOrderTotalBalance()
        {
            double totalBalance = 0;
            double totalOrderAmount = CalculateTotalOrderAmount();
            double totalTaxAmount = Convert.ToDouble(totalTaxTextBox.Text.Substring(1));
            if (double.TryParse(tipsTextbox.Text, out double tipAmount))
            {
                totalBalance = totalOrderAmount + totalTaxAmount + tipAmount;

                CultureInfo cultureInfo = new CultureInfo("en-CA");
                cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
                balanceTextBox.Text = totalBalance.ToString("C", cultureInfo);
            }
        }

        private void InitializeEventHandlers()
        {
            // Attach the event handler for the TextChanged event of the tipsTextbox
            tipsTextbox.TextChanged += TipsTextbox_TextChanged;
        }

        // Handle the TextChanged event of the tipsTextbox to update the balance
        private void TipsTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateOrderTotalBalance();
        }

        private void cashBtn_Click(object sender, RoutedEventArgs e)
        {
            paymentMethod = "cash";
            cashBtn.Background = Brushes.White;
            visaBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            mcBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            amexBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));

        }

        private void visaBtn_Click(object sender, RoutedEventArgs e)
        {
            paymentMethod = "visa";
            cashBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            visaBtn.Background = Brushes.White;
            mcBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            amexBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
        }

        private void mcBtn_Click(object sender, RoutedEventArgs e)
        {
            paymentMethod = "mastercard";
            cashBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            visaBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            mcBtn.Background = Brushes.White;
            amexBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            MessageBox.Show("MC butt");

        }

        private void amexBtn_Click(object sender, RoutedEventArgs e)
        {
            paymentMethod = "amex";
            cashBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            visaBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            mcBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            amexBtn.Background = Brushes.White;
        }

    }
}
