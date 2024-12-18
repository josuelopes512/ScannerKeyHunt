﻿namespace ScannerKeyHunt.Domain.Models
{
    public class JwtToken
    {
        public string Token { get; set; }

        public DateTime Expiration { get; set; }

        public string RefreshToken { get; set; }

        public DateTime ExpirationRefreshToken { get; set; }
    }
}
