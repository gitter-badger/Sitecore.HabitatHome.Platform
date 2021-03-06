﻿using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Linq;

namespace Sitecore.HabitatHome.Website.Test
{
    [TestFixture]
    public class HabitatHomeTest : SeleniumTest
    {
        public class User
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }



        public string Host { get; set; } = "http://habitathome.dev.local";
        public string UserEmail { get; set; } = "test.user@sitecore.net";
        public string UserPassword { get; set; } = "habitat";



        protected static string Capitalize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            if (text.Length == 1)
                return text.ToUpper();
            return text.Substring(0, 1).ToUpper() + text.Substring(1);
        }



        protected void DeleteAccount()
        {
            GoTo(Host);
            Click("MY ACCOUNT");
            Click("Delete Account");
            Click("input[type='submit']");
            AcceptAlert();
        }



        protected User GetUser()
        {
            var user = new User { Email = UserEmail, Password = UserPassword };

            var name = (user.Email ?? "").Split('@')[0];

            char? delimiter = null;
            foreach (char c in "._-")
            {
                if (name.Contains(c))
                {
                    delimiter = c;
                    break;
                }
            }

            if (delimiter == null)
            {
                user.FirstName = Capitalize(name);
                user.LastName = "User";
            }
            else
            {
                user.FirstName = Capitalize(name.Split(delimiter.Value)[0]);
                user.LastName = Capitalize(name.Split(delimiter.Value)[1]);
            }

            return user;
        }



        protected void Login(bool doRegisttrationIfMissing = true)
        {
            User user = GetUser();

            GoTo(Host);
            Click("LOGIN");

            EnterText("#loginEmail", user.Email);
            EnterText("#loginPassword", user.Password);
            Click("input[type='submit']");

            if (!GetElements("Logout").Any() && doRegisttrationIfMissing)
                Register();
        }



        protected static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += " " + unitsMap[number % 10];
                }
            }

            return words;
        }



        protected void Register()
        {
            User user = GetUser();

            GoTo(Host);
            Click("LOGIN");
            Click("CREATE ACCOUNT");

            EnterText("#registerFirstName", user.FirstName);
            EnterText("#registerLastName", user.LastName);
            EnterText("#registerEmail", user.Email);
            EnterText("#registerPassword", user.Password);
            EnterText("#registerConfirmPassword", user.Password);
            Click("input[type='submit']");
        }



        protected Exception Try(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }



        [Test]
        public void TestAccountDeletion()
        {
            Login();
            DeleteAccount();
            TakeScreenshot("01-DeleteAccountResult");
        }



        [Test]
        public void TestNewUser()
        {
            GoTo($"{Host}/landing-pages/bing-smart-home-design");
            TakeScreenshot("01-Bing");

            Click("How to Design Your Smart Home");
            // If cookie warning of doom is present, click confirm to remove it.
            var element = GetElement("div.privacy-warning div.submit a");
            if (element != null)
                Click(element);
            Wait("button.btn-info.sidebar-closed");
            Click("button.btn-info.sidebar-closed");
            Click("Referral");
            TakeScreenshot("02-CampaignActivated");

            Click("a[title='Logo']");
            TakeScreenshot("03-PersonalizedHomepage", "SMART HOME CHECKLIST");
            Wait("button.btn-info.sidebar-closed");
            Click("button.btn-info.sidebar-closed");
            Click("Onsite Behavior");
            var elements = GetElements("#onSiteBehaviorProfileCurrent div.progress-bar.progress-bar-success");
            TakeScreenshot("04-CampaignTriggered", elements.LastOrDefault());

            Click("START HERE");
            Click("label.smart-home");
            TakeScreenshot("05-Guide", "input[value='Next']");

            Click("input[value='Next']");
            Click("label.entire-house");
            Click("input[value='Next']");
            Click("label.n-a");
            Click("input[value='Next']");
            TakeScreenshot("06-RecommendedGuides", "a[href*='dryer-stack-up']");

            Click("a[href*='dryer-stack-up']");
            Wait("button.btn-info.sidebar-closed");
            Click("button.btn-info.sidebar-closed");
            Click("Onsite Behavior");
            elements = GetElements("#onsiteBehaviorPanel div.media div.text-nowrap");
            TakeScreenshot("07-ContentFinderCompleted", elements.FirstOrDefault(e => e.Text.Contains("Content Finder Completed")));
            Click("a[title='Logo']");
            TakeScreenshot("08-PersonalizedHomepage2", "CONTENT FINDER");

            GoTo($"{Host}/en/guides/dryer-stack-up");
            GetElement("input[data-sc-field-name='Email']").SendKeys("test.user@sitecore.net");
            Click("input[value='Sign Me Up!']");
            Wait("button.btn-info.sidebar-closed");
            Click("button.btn-info.sidebar-closed");
            Click("Personal Information");
            TakeScreenshot("09-IdentityEstablished");
            Click("Onsite Behavior");
            elements = GetElements("#onsiteBehaviorPanel div.media div.text-nowrap");
            TakeScreenshot("10-ContentSignup", elements.FirstOrDefault(e => e.Text.Contains("Content Sign up")));
        }



        [Test]
        public void TestRegistration()
        {
            Login(false);
            WaitForDocumentReady();
            var errors = GetElements("div.field-validation-error");
            if (!errors.Any(e => e.Text.Contains("Username or password is not valid")))
                DeleteAccount();

            Register();
            TakeScreenshot("01-RegistrationResult");
            Click("Logout");
            Login();
            TakeScreenshot("02-LoginResult");
        }

    }
}