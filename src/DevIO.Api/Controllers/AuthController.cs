using DevIO.Business.Intefaces;
using DevIO.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace DevIO.Api.Controllers
{
    [Route("api/conta")]
    public class AuthController : MainController
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;    
        public AuthController(INotificador notificador,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager) : base(notificador)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        
        [HttpPost("nova-conta")]
        public async Task<ActionResult> Registrar(RegisterUserViewModel registerUser)
        {
            if(!ModelState.IsValid)
            {
                return CustomResponse(ModelState);
            }
            var user = new IdentityUser
            {
                UserName = registerUser.Email,
                Email = registerUser.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, registerUser.Password);
            if(result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return CustomResponse(registerUser);
            }
            foreach(var error in result.Errors)
            {
                NotificarErro(error.Description);
            }
            
            return CustomResponse(registerUser);
        }


        [HttpPost("entrar")]
        public async Task<ActionResult> Login(LoginUserViewModel loginUser)
        {
            if(!ModelState.IsValid) return CustomResponse(ModelState);

            var result = await _signInManager.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);

            if(result.Succeeded){
                return CustomResponse(loginUser);
            }
            if(result.IsLockedOut){
                NotificarErro("Usuário temporariamente bloqueado por tentativas inválidas");
                return CustomResponse(loginUser);
            }
            NotificarErro("Usuário e senha incorreto");
            return CustomResponse(loginUser);
        }
    }
}
