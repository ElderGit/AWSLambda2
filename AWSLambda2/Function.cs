using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;
using RestSharp;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSLambda2
{
    public class item
    {
        public string birthday { get; set; }
        public string admissionDate { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string id { get; set; }
        public int Age { get; set; }
        public int CompanyTime { get; set; }



    }
    public class Function
    {
        public List<item> listDate;
        private string ApiXerpa_ = "Bearer fHOoWDl4LK1f0Nw5pvOnNTzbWxo_S6GQTPEOLa6wh-c=";
        public string GetProfiles()
        {
           
            var client = new RestClient("https://app.xerpa.com.br/api/g");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", ApiXerpa_);
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
            int Year = DateTime.Today.Year;
            int yearAdmissionDate = int.Parse(Date.Split("-")[0]);
            return Year - yearAdmissionDate;

        }
        public string DateNow()
        {
            var day = DateTime.Today.ToString("d").Split("/");
            var dayDate = day[0] + "/" + day[1];
            return dayDate;
        }
        public List<item> ConvertJsonToItem( string profiles)
        {
            JObject json = JObject.Parse(profiles);
            var j = json["data"]["company"]["profile_search"]["profiles"];
            List<item> items = new List<item>();
            foreach (JObject key in j)
            {
                item item = new item
                {
                    name = key["name"].ToString(),
                    admissionDate = FormatDateInput(key["admissionDate"].ToString()),
                    birthday = FormatDateInput(key["birthday"].ToString()),
                    status = key["status"].ToString(),
                    Age = Years(key["birthday"].ToString()),
                    CompanyTime = Years(key["admissionDate"].ToString())

                };
                items.Add(item);
            }
            return items;
        }
        public List<item> GetProfilesBirthdayToday(List<item> items)
        {
            var BirthdayToday = from key in items
                                where key.birthday == DateNow()
                                select key;
            return BirthdayToday.ToList();
        }
        public List<item> GetProfilesAdmissionToday(List<item> items)
        {
            var AdmissionToday = from key in items
                                 where key.admissionDate == DateNow()
                                 select key;
            return AdmissionToday.ToList();
        }
        public List<item> GetProfilesNextAdmissionMonth(List<item> items)
        {
            var NextAdmission = from key in items
                              where int.Parse(key.admissionDate.Split("/")[1]) == DateTime.Today.Month && int.Parse(key.admissionDate.Split("/")[0]) > DateTime.Today.Day
                              orderby key.admissionDate
                              select key;
            return NextAdmission.ToList();
        }
        public List<item> GetProfilesNextBirthdayMonth(List<item> items)
        {
            var NextBirthdaysMonths = from key in items
                                       where int.Parse(key.birthday.Split("/")[1]) == DateTime.Today.Month && int.Parse(key.birthday.Split("/")[0]) > DateTime.Today.Day
                                       orderby key.birthday
                                       select key;
            return NextBirthdaysMonths.ToList();
        }
     
          

        public string HtmlContent(ArrayList array, List<item> list)
        {
            string newHtml = "";
            string html = "<!doctype html><html lang='en'>  <head>    <!-- Required meta tags -->    <meta charset='utf-8'>    <meta name='viewport' content='width=device-width, initial-scale=1, shrink-to-fit=no'>    <!-- Bootstrap CSS -->    <link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/css/bootstrap.min.css' integrity='sha384-Vkoo8x4CGsO3+Hhxv8T/Q5PaXtkKtu6ug5TOeNV6gBiFeWPGFN9MuhOf23Q9Ifjh' crossorigin='anonymous'>    </head>  <body><table class='table table-borderless'>  <thead>    <tr>           <!-- <th scope='col'><h3 style='margin-left:1px'>Aniversariantes do dia</h3></th> -->          </tr>  </thead>  <tbody>    <tr>       <td><!-- *line* --></td>     </tr>     </tbody></table><br><br><table class='table table-borderless'>  <thead>    <tr>          <!-- <th scope='col'><h3 style='margin-left:1px'>Tempo de Base2</h3></th> -->          </tr>  </thead>  <tbody>    <tr>       <td><!-- *linebase* --></td>     </tr>     </tbody></table><table class='table table-borderless'>  <thead>    <tr>          <!-- <th scope='col'><h3 style='margin-left:1px'>Proximas datas</h3></th> -->          </tr>  </thead>  <tbody>    <tr>       <td><!-- *lineDatesMonth* --></td>     </tr>     </tbody></table>    <!-- Optional JavaScript -->    <!-- jQuery first, then Popper.js, then Bootstrap JS -->    <script src='https://code.jquery.com/jquery-3.4.1.slim.min.js' integrity='sha384-J6qa4849blE2+poT4WnyKhv5vZF5SrPo0iEjwBvKU7imGFAV0wwj1yYfoRSJoZ+n' crossorigin='anonymous'></script>    <script src='https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js' integrity='sha384-Q6E9RHvbIyZFJoft+2mJbHaEWldlvI9IOYy5n3zV9zzTtmI3UksdQRVvoxMfooAo' crossorigin='anonymous'></script>    <script src='https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/js/bootstrap.min.js' integrity='sha384-wfSDF2E50Y2D1uUdj0O3uMBJnjuUD4Ih7YwaYd1iqfktj0Uod8GCExl3Og8ifwB6' crossorigin='anonymous'></script>  </body></html>";
            ArrayList birthday = (ArrayList)array[0];
            ArrayList timeBase2 = (ArrayList)array[1];
            if (birthday.Count != 0)
            {
                var html1 = html.Split("<!-- <th scope='col'><h3 style='margin - left:1px'>Aniversariantes do dia</h3></th> -->");
                html = html1[0] + "\n<th scope='col'><h3 style='margin - left:1px'>Aniversariantes do dia</h3></th>\n" + html1[1];
                html1 = html.Split("<!-- *line* -->");
                foreach (JObject arr in birthday)
                {
                    var name = arr["name"];
                    html1 = html.Split("<!-- *line* -->");
                    newHtml = html1[0] + "<br><strong>" + name + "</strong> está fazendo aniversário hoje. \n <!-- *line* -->\n" + html1[1];
                    html = newHtml;
                }
            }

            if (timeBase2.Count != 0)
            {
                var html1 = html.Split("<!-- <th scope='col'><h3 style='margin-left:1px'>Tempo de Base2</h3></th> -->");
                html = html1[0] + "\n <th scope='col'><h3 style='margin - left:1px'>Tempo de Base2</h3></th> \n" + html1[1];

                foreach (JObject arr in timeBase2)
                {
                    var name = arr["name"];
                    html1 = html.Split("<!-- *linebase* -->");
                    newHtml = html1[0] + "<br><strong>" + name + "</strong> está completando <strong>" + arr["years"] + "</strong>" + ((int)arr["years"] > 1 ? " anos" : " ano") + " de Base2 \n <!-- *linebase* -->\n" + html1[1];
                    html = newHtml;
                }
            }
            if (list.Count != 0)
            {
                var html1 = html.Split("<!-- <th scope='col'><h3 style='margin-left:1px'>Proximas datas</h3></th> -->");
                html = html1[0] + "\n <th scope='col'><h3 style='margin-left:1px'>Próximas datas</h3></th> \n" + html1[1];

                //foreach (item arr in list)
                //{
                //    var name = arr.name;
                //    if (arr.admission)
                //    {
                //        html1 = html.Split("<!-- *lineDatesMonth* -->");
                //        newHtml = html1[0] + "<br><strong>" + name + "</strong> irá completar <strong>" + arr.years + "</strong>" + (arr.years > 1 ? " anos" : " ano") + " de Base2 no dia <strong>" + FormatDateInput(arr.admissionDate) + " </strong> \n <!-- *lineDatesMonth* -->\n" + html1[1];

                //    }
                //    if (arr.birthdayMonth)
                //    {
                //        html1 = html.Split("<!-- *lineDatesMonth* -->");
                //        newHtml = html1[0] + "<br>Aniversário <strong>" + name + " dia: " + FormatDateInput(arr.birthday) + " </strong>\n<!-- *lineDatesMonth* -->\n" + html1[1];

                //    }
                //    html = newHtml;
                //}
            }
            if (timeBase2.Count == 0 && birthday.Count == 0 && list.Count == 0)
            {
                return "";
            }

            return newHtml;
        }
        public string Execute(string html)
        {

            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("noreplay@base2.com.br", "nao-responda - Aniversariantes");
            var subject = "Aniversariantes " + DateTime.Today.ToString("d");
            var to = new EmailAddress("elder.barbosa.lima@gmail.com", "-");
            var plainTextContent = "";
            var htmlContent = html;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = client.SendEmailAsync(msg);
            return response.ToString();
        }
        public string FunctionHandler()
        {


            string profiles = GetProfiles();

            //var html = HtmlContent(profilesResult, listDate);
            //if (html != "")
            //{
            //    return (Execute(html));
            //}
            //else
            //{
            //    return null;
            //}
            return null;
        }
    }
}
