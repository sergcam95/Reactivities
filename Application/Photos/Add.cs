using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Photos {
    public class Add {
        public class Command : IRequest<Photo> {
            public IFormFile File { get; set; }
        }

        public class Handler : IRequestHandler<Command, Photo> {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly IPhotoAccessor _photoAccessor;

            public Handler (DataContext context, IUserAccessor userAccessor, IPhotoAccessor photoAccessor) {
                _photoAccessor = photoAccessor;
                _userAccessor = userAccessor;
                _context = context;
            }

            public async Task<Photo> Handle (Command request, CancellationToken cancellationToken) {

                var photoUploadResult = _photoAccessor.AddPhoto (request.File);

                var user = await _context.Users.SingleOrDefaultAsync (x =>
                    x.UserName == _userAccessor.GetCurrentUsername ());

                var photo = new Photo {
                    Url = photoUploadResult.Url,
                    Id = photoUploadResult.PublicId,
                };

                if (!user.Photos.Any (x => x.IsMain))
                    photo.IsMain = true;

                // Because we got the user using _context, this Add method
                // is modifying the record and using SaveChangesAsync() it is 
                // updated in the database
                user.Photos.Add (photo);

                // 1) Save the changes into the database
                // 2) It verifies if it was successfully saved
                var success = await _context.SaveChangesAsync () > 0;

                if (success) return photo;

                throw new Exception ("Problem saving changes");
            }
        }
    }
}