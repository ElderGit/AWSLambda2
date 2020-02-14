using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net.Mail;
using System.Net.Mime;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.IO;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSLambda2
{
    public class Function
    {
        public string getProfile()
        {
            var client = new RestClient("https://app.xerpa.com.br/api/g");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", "Bearer fHOoWDl4LK1f0Nw5pvOnNTzbWxo_S6GQTPEOLa6wh-c=");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "{\"query\":\"query{company(id:\\\"4930\\\"){profile_search(query:\\\"page_size=3000&page=1&source=false&filter[status][]=active%2Cin-admission%2Coffboarding\\\"){profiles{id,name,username,birthday,status,admissionDate}}}}\",\"variables\":{\"companyId\":4930}}",
                       ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return response.Content;
        }
        public string FormatDateInput(string input)
        {
            var inputDate = input.Split("-");
            var inputReturn = inputDate[2] + "/" + inputDate[1];
            return inputReturn;
        }
        public int Years(string Date)
        {
            var dayDate = DateTime.Today;
            int year = dayDate.Year;
            var splitAdmissionDate = Date.Split("-");
            var yearAdmissionDate = int.Parse(splitAdmissionDate[0]);
            return year - yearAdmissionDate;

        }
        public string DateNow()
        {
            var day = DateTime.Today.ToString("d");
            var daySplit = day.Split("/");
            var dayDate = daySplit[0] + "/" + daySplit[1];
            return dayDate;
        }
        public ArrayList getBirthDayProfiles(string profiles)
        {
            JObject json = JObject.Parse(profiles);
          
            var dayDate =DateNow();

            var profilesBirthDay = new ArrayList();
            var profilesAdmissionDate = new ArrayList();
            var arrayReturn = new ArrayList();

            var profilesJson = json["data"]["company"]["profile_search"]["profiles"];
            foreach (JObject pro in profilesJson)
            {
                var birthday = pro["birthday"] != null ? (string)pro["birthday"] : null;
                var admissionDate = pro["admissionDate"] != null ? (string)pro["admissionDate"] : null;
                var birthdayDate = "";
               
                if (birthday != null)
                {
                    birthdayDate = FormatDateInput(birthday);
                    if (birthdayDate == "09/06")
                    {
                        var year = Years((string)pro["birthday"]);
                        pro.Add("years",year);
                        profilesBirthDay.Add(pro);
                    }
                }
                
                if (pro["admissionDate"] != null)
                {
                    admissionDate = FormatDateInput(admissionDate);
                    if (admissionDate == "19/03")
                    {
                        var year = Years((string)pro["admissionDate"]);
                        pro.Add("years", year);
                        profilesAdmissionDate.Add(pro);
                    }
                }
           
            }
            arrayReturn.Add(profilesBirthDay);
            arrayReturn.Add(profilesAdmissionDate);
            
            return arrayReturn;
        }
        public string HtmlContent(ArrayList array)
        {
            var birthday = array[0];
            foreach(ArrayList arr in array)
            {
                return arr.ToString();
            }
            return File.ReadAllText("Index.txt");
        }
        public void Execute()
        {
           
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("elder.lima@base2.com.br", "Example User");
            var subject = "Sending with SendGrid is Fun";
            var to = new EmailAddress("elder.barbosa.lima@gmail.com", "Example User");
            var plainTextContent = "and easy to do anywhere, even with C#";
            var htmlContent = "";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = client.SendEmailAsync(msg);
        }
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public ArrayList FunctionHandler()
        {
           
          
            string profiles = getProfile();
           
            var profilesResult = getBirthDayProfiles(profiles);
            var html = HtmlContent(profilesResult);
            Execute();


            return (profilesResult);
        }
    }
}
