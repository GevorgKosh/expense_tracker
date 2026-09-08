using AutoMapper;
using ExpenseTracker.Dto;

namespace ExpenseTracker.Mapping;

public class MappingProfile: Profile
{
    public  MappingProfile()
    {
        CreateMap<Expense, ExpenseResponse>();
        CreateMap<Category, CategoryResponse>();
    }
}