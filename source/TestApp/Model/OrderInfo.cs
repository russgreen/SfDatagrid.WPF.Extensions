using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TestApp.Model;

internal partial class OrderInfo : ObservableValidator
{
    [ObservableProperty]
    private string _customerId;

    [ObservableProperty]
    private string _country;

    [ObservableProperty]
    private string _customerName;

    [ObservableProperty]
    private string _shippingCity;

    [ObservableProperty]
    private DateTime _orderDate;


    [ObservableProperty]
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    private int _quantity;

    public OrderInfo(string customerName, string country, string
        customerId, string shipCity, int quantity, DateTime orderDate)
    {
        CustomerName = customerName;
        Country = country;
        CustomerId = customerId;
        ShippingCity = shipCity;
        OrderDate = orderDate;
        Quantity = quantity;

    }
}
