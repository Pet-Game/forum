using Microsoft.AspNetCore.Identity.UI.Services;
using NETCore.MailKit.Core;

namespace PetGameForum.Services; 

public class MailKitSender : IEmailSender {
	private IEmailService emailService; 
	
	public MailKitSender(IEmailService emailService) {
		this.emailService = emailService;
	}

	public Task SendEmailAsync(string email, string subject, string message) {
		return emailService.SendAsync(email, subject, message);
	}

	public void SendEmail(string email, string subject, string message) {
		emailService.Send(email, subject, message);
	}
}