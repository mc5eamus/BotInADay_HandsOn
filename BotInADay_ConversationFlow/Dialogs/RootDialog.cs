using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BotInADay_ConversationFlow.Model;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using AdaptiveCards;

namespace BotInADay_ConversationFlow.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            switch (activity.Text)
            {
                case "guess":
                    var number = (new Random(Guid.NewGuid().GetHashCode())).Next(1, 101);
                    context.Call(new GuessTheNumberDialog(number), Guessed);
                    return;
                case "guess++":
                    var guessDialog = FormDialog.FromForm(GuessModel.BuildForm, FormOptions.PromptInStart);
                    context.Call(guessDialog, Guessed);
                    return;
            }

            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;

            // return our reply to the user
            await context.PostAsync($"You sent {activity.Text} which was {length} characters");

            context.Wait(MessageReceivedAsync);
        }

        private async Task Guessed(IDialogContext context, IAwaitable<int> result)
        {
            var count = await result;
            string statInfo = null;
            if (context.UserData.TryGetValue("guess_stats", out int recent))
            {
                int diff = count - recent;
                switch (Math.Sign(diff))
                {
                    case -1:
                        statInfo = $", {-diff} less than last time";
                        break;
                    case 1:
                        statInfo = $", {diff} more than last time";
                        break;
                    case 0:
                        statInfo = ", exactly like last time";
                        break;
                }
            }
            context.UserData.SetValue("guess_stats", count);

            var msg = $"Good quess. It took you {count} rounds{statInfo}!";

            var stats = GetGuessStatisticsCard(count, recent, msg);

            var message = GetAdaptiveCardMessage(context, stats, "Guess Game Stats");

            await context.PostAsync(message);


            context.Wait(MessageReceivedAsync);
        }

        private async Task Guessed(IDialogContext context, IAwaitable<GuessModel> result)
        {
            var guess = await result;
            await context.PostAsync($"Good quess. It took you {guess.Counter} rounds!");
            context.Wait(MessageReceivedAsync);
        }

        private IMessageActivity GetAdaptiveCardMessage(IDialogContext context, AdaptiveCards.AdaptiveCard card, string cardName)
        {
            var message = context.MakeMessage();
            if (message.Attachments == null)
                message.Attachments = new List<Attachment>();
            var attachment = new Attachment()
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive",
                Name = cardName
            };
            message.Attachments.Add(attachment);
            return message;
        }

        private static AdaptiveCard GetGuessStatisticsCard(int actual, int last, string description)
        {
            var card = LoadCardTemplate();
            var timestampElement = ((card.Body[0] as AdaptiveCards.AdaptiveContainer).Items[1] as AdaptiveColumnSet)
                .Columns[1].Items[1] as AdaptiveTextBlock;
            timestampElement.Text = DateTime.Today.ToString("dd MMM");

            var descriptionElement = (card.Body[1] as AdaptiveCards.AdaptiveContainer)?
                .Items[0] as AdaptiveTextBlock;
            descriptionElement.Text = description;

            var factSet = (card.Body[1] as AdaptiveCards.AdaptiveContainer)?
                .Items[1] as AdaptiveFactSet;

            factSet.Facts[0].Value = last.ToString();
            factSet.Facts[1].Value = actual.ToString();

            if (last == 0)
            {
                factSet.Facts.RemoveAt(0);
            }

            return card;
        }

        private static AdaptiveCard LoadCardTemplate()
        {
            AdaptiveCard card;

            using (Stream stream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream("BotInADay_ConversationFlow.statscard.json"))
            using (StreamReader reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                AdaptiveCardParseResult result = AdaptiveCard.FromJson(json);
                card = result.Card;
            }
            return card;
        }

    }
}