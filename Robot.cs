using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;

namespace TradeRobot
{
    public class Robot
    {
        NewsApiClient newsApiClient;

        public Robot()
        {
            string APIKey = "3d0ba6c2bd1f43c580d001b40862ea20";

            newsApiClient = new NewsApiClient(APIKey);
        }

        public async Task<string> getForecast(string charCode, string name)
        {
            string message = name + "\n";
            var articlesResponse = await newsApiClient.GetEverythingAsync(new EverythingRequest
            {
                Q = "(курс) AND " + "((" + charCode + ')' + " OR " + '(' + name + "))",
                SortBy = SortBys.Relevancy,
                Language = Languages.RU,
                From = DateTime.Now.AddMonths(-1)
            });

            if (articlesResponse.Status == Statuses.Ok)
            {
                message += "Всего запросов: " + articlesResponse.TotalResults + ".\n";

                int score = 0;
                string[] positiveKeyWords = { "растет", "вырастет", "выше", "высоко",
                                              "поднимается", "подскочит", "подъем",
                                              "повышается", "увеличится", "увеличение",
                                              "вверх", "подъем", "поднимал", "расширение",
                                              "нарост", "расцвет", "расширение",
                                              "продвижение вверх", "прогресс", };
                string[] negativeKeyWords = { "спад", "опускание", "обрушение", "обрушит",
                                              "обрушится", "снижение", "понижение",
                                              "падающая тенденция", "падающий тренд",
                                              "снижающаяся динамика",
                                              "понижение стоимости", "снижение значения",
                                              "негативная динамика", "обвал", "спуск", };

                foreach (var article in articlesResponse.Articles)
                {
                    foreach(var keyWord in positiveKeyWords)
                    {
                        if (article.Title.Contains(keyWord) || article.Description.Contains(keyWord))
                        {
                            score++;
                        }
                    }
                    foreach (var keyWord in negativeKeyWords)
                    {
                        if (article.Title.Contains(keyWord) || article.Description.Contains(keyWord))
                        {
                            score--;
                        }
                    }
                }

                if (score > 0)
                {
                    message += "По прогнозам, курс должен повыситься. Советую покупать.";
                }
                else if (score < 0)
                {
                    message += "По прогнозам, курс должен понизиться. Советую продавать";
                }
                else
                {
                    message += "Недостаточно информации, чтобы спрогнозировать движение курса.";
                }
            }
            else
            {
                message += "Нет ответа от API.";
            }

            return message;
        }
        public string getForecastTest(List<Article> news)
        {
            string message = "";

            int score = 0;
            string[] positiveKeyWords = { "растет", "вырастет", "выше", "высоко",
                                          "поднимается", "подскочит", "подъем",
                                          "повышается", "увеличится", "увеличение",
                                          "вверх", "подъем", "поднимал", "расширение",
                                          "нарост", "расцвет", "расширение",
                                          "продвижение вверх", "прогресс", };
            string[] negativeKeyWords = { "спад", "опускание", "обрушение", "обрушит",
                                          "обрушится", "снижение", "понижение",
                                          "падающая тенденция", "падающий тренд",
                                          "снижающаяся динамика",
                                          "понижение стоимости", "снижение значения",
                                          "негативная динамика", "обвал", "спуск", };

            foreach (var article in news)
            {
                foreach (var keyWord in positiveKeyWords)
                {
                    if (article.Title.Contains(keyWord) || article.Description.Contains(keyWord))
                    {
                        score++;
                    }
                }
                foreach (var keyWord in negativeKeyWords)
                {
                    if (article.Title.Contains(keyWord) || article.Description.Contains(keyWord))
                    {
                        score--;
                    }
                }
            }

            if (score > 0)
            {
                message += "Повыситься";
            }
            else if (score < 0)
            {
                message += "Понизиться";
            }
            else
            {
                message += "Неизвестно";
            }

            return message;
        }
    }
}
