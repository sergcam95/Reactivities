using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Application.Profiles
{
    public class Details
    {
        public class Query : IRequest<Profile>
        {
            public string Username { get; set; }
        }

        public class Handler : IRequestHandler<Query, Profile>
        {

            private readonly IProfileReader __profileReader;

            public Handler (IProfileReader _profileReader)
            {
                __profileReader = _profileReader;
            }

            public async Task<Profile> Handle (Query request,
                CancellationToken cancellationToken)
            {

                return await __profileReader.ReadProfile (request.Username);
            }
        }
    }
}