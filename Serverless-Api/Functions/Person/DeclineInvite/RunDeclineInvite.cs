using Domain;
using Eveneum;
using CrossCutting;
using Domain.Events;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using static Domain.ServiceCollectionExtensions;
using Domain.Interfaces;

namespace Serverless_Api
{
    public partial class RunDeclineInvite
    {
        private readonly Person _user;
        private readonly IPersonRepository _peopleStore;
        private readonly IInvitesService _invitesService;

        public RunDeclineInvite(Person user, IPersonRepository peopleStore, IBbqRepository bbqRepository, IInvitesService invitesService)
        {
            _user = user;
            _peopleStore = peopleStore;
            _invitesService = invitesService;
        }

        [Function(nameof(RunDeclineInvite))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "person/invites/{inviteId}/decline")] HttpRequestData req, string inviteId)
        {
            var person = await _peopleStore.GetAsync(_user.Id);            

            // caso o ultimo invite for com o status Accepted, impede que o usuario aceite para evitar repetições
            if (person?.Invites?.FirstOrDefault(e => e.Id == inviteId)?.Status == InviteStatus.Declined)
            {
                return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
            }

            if (person == null)
                return req.CreateResponse(System.Net.HttpStatusCode.NoContent);

            await _invitesService.DeclineBbqInvite(person, inviteId);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
        }
    }
}
