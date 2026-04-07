using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagementService.Application.Common.Exceptions;

public class BadRequestException : Exception
{
    public List<string> Errors { get; }

    public BadRequestException(string message) : base(message)
        => Errors = new List<string> { message };

    public BadRequestException(List<string> errors)
        : base("Bad request")
        => Errors = errors;
}
