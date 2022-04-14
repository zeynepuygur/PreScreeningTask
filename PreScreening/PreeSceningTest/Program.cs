using Newtonsoft.Json.Linq;
using PreScreening.Controllers;
using PreScreening.Models;
using PreScreening.Services;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PreeSceningTest
{
    class Program
    {
        static void Main(string[] args)
        {
            PreeSceningTest test = new PreeSceningTest();
            test.TestMain();
        }
    }
}

