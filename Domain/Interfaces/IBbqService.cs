using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IBbqService
    {
        Task<Bbq?> ThereIsSomeoneElseInTheMood(DateTime Date, string Reason, bool IsTrincasPaying);
        Task<Bbq?> BbqStatusUpdated(Bbq Churras, bool GonnaHappen, bool TrincaWillPay);
        Task<List<object>> GetProposedBbqs(string UserId);
        Task<HttpStatusCode> GetShopList(Bbq? Churras, Person Person);



    }
}
