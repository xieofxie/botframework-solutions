using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using HospitalitySkill.Models;
using HospitalitySkill.Responses.GetReservation;
using HospitalitySkill.Responses.Shared;
using HospitalitySkill.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Solutions.Responses;
using Microsoft.Bot.Schema;

namespace HospitalitySkill.Dialogs
{
    public class DeviceControlDialog : HospitalityDialogBase
    {
        private HotelService _hotelService;

        public DeviceControlDialog(
            BotSettings settings,
            BotServices services,
            ResponseManager responseManager,
            ConversationState conversationState,
            UserState userState,
            HotelService hotelService,
            IBotTelemetryClient telemetryClient)
            : base(nameof(DeviceControlDialog), settings, services, responseManager, conversationState, userState, hotelService, telemetryClient)
        {
            var deviceControl = new WaterfallStep[]
            {
                HasCheckedOut,
                DeviceControl
            };

            _hotelService = hotelService;

            AddDialog(new WaterfallDialog(nameof(DeviceControlDialog), deviceControl));

            ThisIntent = Luis.HospitalityLuis.Intent.DeviceControl;
        }

        private async Task SendActionToDevice(WaterfallStepContext sc, string device, string control)
        {
            var actionEvent = sc.Context.Activity.CreateReply();
            actionEvent.Type = ActivityTypes.Event;
            actionEvent.Name = device;
            actionEvent.Value = control;

            await sc.Context.SendActivityAsync(actionEvent);
        }

        private async Task<DialogTurnResult> DeviceControl(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var convState = await StateAccessor.GetAsync(sc.Context, () => new HospitalitySkillState());
            var device = convState.LuisResult.Entities.Device?[0][0];
            var control = convState.LuisResult.Entities.Control?[0][0];
            Activity reply = null;

            if (control != null && (device == null || device == "temperature"))
            {
                if (control == "higher")
                {
                    reply = sc.Context.Activity.CreateReply("Ok, temperature will be higher.");
                    device = "temperature";
                }
                else if (control == "lower")
                {
                    reply = sc.Context.Activity.CreateReply("Ok, temperature will be lower.");
                    device = "temperature";
                }
            }
            else if (control != null && device == "light")
            {
                if (control == "on")
                {
                    reply = sc.Context.Activity.CreateReply("Ok, lights are turned on.");
                }
                else if (control == "off")
                {
                    reply = sc.Context.Activity.CreateReply("Ok, lights are turned off.");
                }
            }

            if (reply == null)
            {
                await sc.Context.SendActivityAsync(ResponseManager.GetResponse(SharedResponses.DidntUnderstandMessage));
            }
            else
            {
                await sc.Context.SendActivityAsync(reply);
                await SendActionToDevice(sc, device, control);
            }

            return await sc.NextAsync();
        }
    }
}
