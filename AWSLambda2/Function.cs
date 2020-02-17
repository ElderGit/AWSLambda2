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
        public string day { get; set; }
        public bool admission { get; set; }
        public bool birthdayMonth { get; set; }
        public int years { get; set; }

    }
    public class Function
    {
        public List<item> listDate;
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
            int Year = DateTime.Today.Year;
            int yearAdmissionDate = int.Parse(Date.Split("-")[0]);
            return Year - yearAdmissionDate;

        }
        public string DateNow()
        {
            var day = DateTime.Today.ToString("d").Split("-");
            var dayDate = day[0] + "/" + day[1];
            return dayDate;
        }
        public ArrayList getBirthDayProfiles(string profiles)
        {
            JObject json = JObject.Parse(profiles);
            var j = json["data"]["company"]["profile_search"]["profiles"]; ;

            var dayDate = DateNow();

            ArrayList profilesBirthDay = new ArrayList();
            ArrayList profilesAdmissionDate = new ArrayList();
            ArrayList arrayReturn = new ArrayList();
            ArrayList DatesMonth = new ArrayList();

            var profilesJson = json["data"]["company"]["profile_search"]["profiles"];
            foreach (JObject pro in profilesJson)
            {
                var birthday = pro["birthday"] != null ? (string)pro["birthday"] : null;
                var admissionDate = pro["admissionDate"] != null ? (string)pro["admissionDate"] : null;

                if (birthday != null)
                {
                    if (FormatDateInput(birthday) == dayDate)
                    {
                        var year = Years((string)pro["birthday"]);
                        pro.Add("years", year);
                        profilesBirthDay.Add(pro);
                    }
                }

                {
                    admissionDate = FormatDateInput(admissionDate);
                    if (admissionDate == dayDate)
                    {
                        var year = Years((string)pro["admissionDate"]);
                        pro.Add("years", year);
                        profilesAdmissionDate.Add(pro);
                    }
                }
                if ((string)pro["birthday"] != null)
                {
                    string monthBirthday = (string)pro["birthday"];
                    var monthBirthday_ = monthBirthday.Split("-");
                    int intMonthBirthday = int.Parse(monthBirthday_[1]);
                    var monthNow = DateTime.Today.Month;
                    var monthAdmissionDate = pro["admissionDate"].ToString().Split("-");
                    int intMonthAdmissionDate = int.Parse(monthAdmissionDate[1]);
                    if (intMonthAdmissionDate == monthNow)
                    {
                        int day_ = int.Parse(monthAdmissionDate[2]);
                        var dayNow_ = DateTime.Today.Day;
                        if (day_ > dayNow_)
                        {
                            var year = Years((string)pro["admissionDate"]);
                            pro.Add("admission", true);
                            pro.Add("birthdayMonth", false);
                            pro.Add("years", year);
                            pro.Add("day", day_);
                            DatesMonth.Add(pro);

                        }
                    }
                    if (intMonthBirthday == monthNow)
                    {
                        int day = int.Parse(monthBirthday_[2]);
                        var dayNow = DateTime.Today.Day;
                        if (day > dayNow)
                        {
                            var year = Years((string)pro["birthday"]);
                            pro.Add("birthdayMonth", true);
                            pro.Add("admission", false);
                            pro.Add("years", year);
                            pro.Add("day", day);
                            DatesMonth.Add(pro);
                        }
                    }

                }

            }

            var collection = new List<item>();
            foreach (JObject value in DatesMonth)
            {

                item item = new item
                {
                    name = value["name"].ToString(),
                    admissionDate = value["admissionDate"].ToString(),
                    birthday = value["birthday"].ToString(),
                    id = value["id"].ToString(),
                    status = value["status"].ToString(),
                    day = value["day"].ToString(),
                    birthdayMonth = (bool)value["birthdayMonth"],
                    admission = (bool)value["admission"],
                    years = (int)value["years"]

                };
                collection.Add(item);
            }

            listDate = collection.OrderBy(x => x.day).ToList();
            arrayReturn.Add(profilesBirthDay);
            arrayReturn.Add(profilesAdmissionDate);

            return arrayReturn;
        }
        //int IComparer.Compare(object x, object y)
        //{
        //    Object arrA = (Object)x;
        //    Object arrB = (Object)x;
        //    return arrA.Day.CompareTo(arrB.Day);

        //}


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

                foreach (item arr in list)
                {
                    var name = arr.name;
                    if (arr.admission)
                    {
                        html1 = html.Split("<!-- *lineDatesMonth* -->");
                        newHtml = html1[0] + "<br><strong>" + name + "</strong> irá completar <strong>" + arr.years + "</strong>" + (arr.years > 1 ? " anos" : " ano") + " de Base2 no dia <strong>" + FormatDateInput(arr.admissionDate) + " </strong> \n <!-- *lineDatesMonth* -->\n" + html1[1];

                    }
                    if (arr.birthdayMonth)
                    {
                        html1 = html.Split("<!-- *lineDatesMonth* -->");
                        newHtml = html1[0] + "<br>Aniversário <strong>" + name + " dia: " + FormatDateInput(arr.birthday) + " </strong>\n<!-- *lineDatesMonth* -->\n" + html1[1];

                    }
                    html = newHtml;
                }
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


            string profiles = getProfile();

            var profilesResult = getBirthDayProfiles(profiles);
            var html = HtmlContent(profilesResult, listDate);
            if (html != "")
            {
                return (Execute(html));
            }
            else
            {
                return null;
            }
        }
    }
}
