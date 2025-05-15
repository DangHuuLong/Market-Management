using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class PaymentController : Controller
{
    private readonly IConfiguration _configuration;

    public PaymentController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost] //route /vnpay_payment
    public IActionResult ProcessPayment(double amount, string billid)
    {
        // Lấy thông tin cấu hình
        string vnp_TmnCode = _configuration["VNPay:TmnCode"];
        string vnp_HashSecret = _configuration["VNPay:HashSecret"];
        string vnp_Url = _configuration["VNPay:BaseUrl"];
        string vnp_Returnurl = _configuration["VNPay:ReturnUrl"];

        // Tạo tham số cho VNPAY
        var vnp_Params = new Dictionary<string, string>
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", vnp_TmnCode },
            { "vnp_Amount", ((int)(amount * 100)).ToString() },
            { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", "127.0.0.1" },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", "Thanh toan don hang: " + billid },
            { "vnp_OrderType", "billpayment" },
            { "vnp_ReturnUrl", vnp_Returnurl },
            { "vnp_TxnRef", billid }
        };

        var sortedParams = vnp_Params.OrderBy(k => k.Key).ToDictionary(k => k.Key, k => k.Value);
        var queryString = string.Join("&", sortedParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

        // Tạo chữ ký (Secure Hash)
        string signData = queryString;
        string vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);
        queryString += "&vnp_SecureHash=" + vnp_SecureHash;

        // Chuyển hướng đến URL thanh toán của VNPAY
        string paymentUrl = vnp_Url + "?" + queryString;
        return Redirect(paymentUrl);
    }

    // Hàm mã hóa HMAC-SHA512
    private string HmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            byte[] hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }
        return hash.ToString();
    }
    public IActionResult Return()
    {
        string vnp_HashSecret = _configuration["VNPay:HashSecret"];
        var vnp_Params = new Dictionary<string, string>();

        // Lấy tất cả tham số trả về từ VNPAY
        foreach (var key in Request.Query.Keys)
        {
            if (key.StartsWith("vnp_"))
            {
                vnp_Params.Add(key, Request.Query[key]);
            }
        }

        // Xác minh chữ ký
        string vnp_SecureHash = Request.Query["vnp_SecureHash"];
        vnp_Params.Remove("vnp_SecureHash");
        var sortedParams = vnp_Params.OrderBy(k => k.Key).ToDictionary(k => k.Key, k => k.Value);
        var queryString = string.Join("&", sortedParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        string checkSum = HmacSHA512(vnp_HashSecret, queryString);

        if (checkSum == vnp_SecureHash)
        {
            if (vnp_Params["vnp_ResponseCode"] == "00")
            {
                ViewBag.Message = "Thanh toán thành công! Mã giao dịch: " + vnp_Params["vnp_TxnRef"];
            }
            else
            {
                ViewBag.Message = "Thanh toán thất bại! Mã lỗi: " + vnp_Params["vnp_ResponseCode"];
            }
        }
        else
        {
            ViewBag.Message = "Chữ ký không hợp lệ!";
        }

        return View();
    }
}
