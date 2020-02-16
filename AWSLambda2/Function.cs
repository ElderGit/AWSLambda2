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

           ArrayList profilesBirthDay = new ArrayList();
            ArrayList profilesAdmissionDate = new ArrayList();
            ArrayList arrayReturn = new ArrayList();

            var profilesJson = json["data"]["company"]["profile_search"]["profiles"];
            foreach (JObject pro in profilesJson)
            {
                var birthday = pro["birthday"] != null ? (string)pro["birthday"] : null;
                var admissionDate = pro["admissionDate"] != null ? (string)pro["admissionDate"] : null;
                var birthdayDate = "";
               
                if (birthday != null)
                {
                    birthdayDate = FormatDateInput(birthday);
                    if (birthdayDate == dayDate)
                    {
                        var year = Years((string)pro["birthday"]);
                        pro.Add("years",year);
                        profilesBirthDay.Add(pro);
                    }
                }
                
                if (pro["admissionDate"] != null)
                {
                    admissionDate = FormatDateInput(admissionDate);
                    if (admissionDate == dayDate)
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
            string newHtml = "";
            string html = File.ReadAllText("Index.txt");
            
             ArrayList birthday = (ArrayList) array[0];
            if (birthday.Count != 0)
            {
                foreach (JObject arr in birthday)
                {
                    var name = arr["name"];
                    var html1 = html.Split("<!-- *line* -->");
                    newHtml = html1[0] + "<br><strong>" + name + "</strong> está completando <strong>" + arr["years"] + "</strong> anos de idade \n <!-- *line* -->\n" + html1[1];
                    html = newHtml;
                }
            }
            ArrayList timeBase2 = (ArrayList)array[1];
            foreach (JObject arr in timeBase2)
            {
                var name = arr["name"];
                var html1 = html.Split("<!-- *linebase* -->");
                newHtml = html1[0] + "<br><strong>" + name + "</strong> está completando <strong>" + arr["years"] + "</strong> anos de Base2 \n <!-- *linebase* -->\n" + html1[1];
                html = newHtml;
            }
            return newHtml;
        }
        public string Execute(string html)
        {
           
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("noreplay@base2.com.br", "nao-responda - Aniversariantes");
            var subject = "Aniversariantes "+DateTime.Today.ToString("d");
            var to = new EmailAddress("elder.barbosa.lima@gmail.com", "-");
            var plainTextContent = "";
            var htmlContent = html;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = client.SendEmailAsync(msg);
            return response.ToString();
        }
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string FunctionHandler()
        {
           
          
            string profiles = getProfile();
           
            var profilesResult = getBirthDayProfiles(profiles);
            var html = HtmlContent(profilesResult);
            return (Execute(html));
        }
    }
}
