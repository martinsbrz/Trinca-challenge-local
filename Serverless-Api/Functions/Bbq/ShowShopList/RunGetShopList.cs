using Domain.Entities;
using Domain.Events;
using Domain.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Serverless_Api.RunAcceptInvite;
using Eveneum;
using CrossCutting;
using Domain.Interfaces;
using System.Net;

namespace Serverless_Api.Functions.Bbq.ShowShopList
{
    public partial class RunGetShopList
    {
        public IBbqRepository _bbqRepository { get; set; }
        public IPersonRepository _personRepository { get; set; }
        public SnapshotStore  _snapshots { get; set; }
        public Person _user { get; set; }

        private readonly IBbqService _bbqService;

        public RunGetShopList(IBbqRepository bbqRepository, Person user, SnapshotStore snapshots, IPersonRepository personRepository, IBbqService bbqService)
        {
            _bbqRepository = bbqRepository;
            _user = user;
            _snapshots = snapshots;
            _personRepository = personRepository;
            _bbqService = bbqService;
        }

        [Function(nameof(RunGetShopList))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "churras/{inviteId}/shopList")] HttpRequestData req, string inviteId)
        {
            var person = await _personRepository.GetAsync(_user.Id);
            var bbq = await _bbqRepository.GetAsync(inviteId);
            var status = await _bbqService.GetShopList(bbq, person);

            return await req.CreateResponse(status, status == HttpStatusCode.OK ? bbq.TakeSnapshot() : null);
        }

    }
}
