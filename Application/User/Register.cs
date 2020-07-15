using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Application.Validators;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User {
    public class Register {

        // Commands usually should never return anything
        // In this case, we want to return the user's info 
        // because we do not want the client to login afterwards
        public class Command : IRequest<User> {
            public string DisplayName { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command> {
            public CommandValidator () {
                RuleFor (x => x.DisplayName).NotEmpty ();
                RuleFor (x => x.Username).NotEmpty ();
                RuleFor (x => x.Email).NotEmpty ().EmailAddress ();
                RuleFor (x => x.Password).Password ();
            }
        }

        public class Handler : IRequestHandler<Command, User> {
            private readonly DataContext _context;
            private readonly UserManager<AppUser> _userManager;
            private readonly IJwtGenerator _jwtGenerator;
            public Handler (DataContext context, UserManager<AppUser> userManager, IJwtGenerator jwtGenerator) {
                _jwtGenerator = jwtGenerator;
                _userManager = userManager;
                _context = context;
            }

            public async Task<User> Handle (Command request, CancellationToken cancellationToken) {

                // Verifies if the email is unique
                if (await _context.Users.AnyAsync (x => x.Email == request.Email))
                    throw new RestException (HttpStatusCode.BadRequest, new { Email = "Email already exists" });

                // Verifies if the username is unique
                if (await _context.Users.AnyAsync (x => x.UserName == request.Username))
                    throw new RestException (HttpStatusCode.BadRequest, new { Username = "Username already exists" });

                var user = new AppUser {
                    DisplayName = request.DisplayName,
                    Email = request.Email,
                    UserName = request.Username,
                };

                // 1) Save the changes into the database
                // 2) It verifies if it was successfully saved
                var result = await _userManager.CreateAsync (user, request.Password);

                if (result.Succeeded) {
                    return new User {
                        DisplayName = user.DisplayName,
                            Token = _jwtGenerator.CreateToken (user),
                            Username = user.UserName,
                            Image = null
                    };
                }

                throw new Exception ("Problem creating user");
            }
        }
    }
}