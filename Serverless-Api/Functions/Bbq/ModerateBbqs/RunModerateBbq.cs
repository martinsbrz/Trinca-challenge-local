using CrossCutting;
using Domain.Entities;
using Domain.Events;
using Domain.Interfaces;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Runtime.CompilerServices;

namespace Serverless_Api
{
    public partial class RunModerateBbq
    {
        private readonly SnapshotStore _snapshots;

        private readonly IBbqRepository _bbqStore;

        private readonly IInvitesService  _invitesService;

        public RunModerateBbq(IBbqRepository bbqStore, SnapshotStore snapshots, IInvitesService invitesService)
        {
            _snapshots = snapshots;
            _bbqStore = bbqStore;
            _invitesService = invitesService;
        }

        [Function(nameof(RunModerateBbq))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "churras/{id}/moderar")] HttpRequestData req, string id)
        {

            var moderationRequest = await req.Body<ModerateBbqRequest>();

            var bbq = await _bbqStore.GetAsync(id);

            await _invitesService.SendBbqResponse(bbq, moderationRequest.GonnaHappen);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, bbq.TakeSnapshot());
        }
    }
}
