using Microsoft.EntityFrameworkCore;
using pixAPI.Data;
using pixAPI.Models;

namespace pixAPI.Repositories;

public class UserRepository(AppDBContext context)
{
  private readonly AppDBContext _context = context;

  public async Task<User?> GetUserByCPF(string CPF)
  {
    return await _context.User.FirstOrDefaultAsync(u => u.CPF.Equals(CPF));
  }

  public async Task<User?> GetUserById(int id)
  {
    return await _context.User.FirstOrDefaultAsync(u => u.Id.Equals(id));
  }
}