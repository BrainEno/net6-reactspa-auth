namespace ReactSpa_Backend.Controllers;

using Microsoft.AspNetCore.Mvc;
using ReactSpa_Backend.Entities;

[Controller]
public abstract class BaseController : ControllerBase
{
    //Returns the current user
    public User Account => (User)HttpContext.Items["User"];
}