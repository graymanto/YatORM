﻿using System.Configuration;

namespace YatORM.Tests.Settings
{
    public static class TestSettings
    {
        private static readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;

        public static string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }
    }
}