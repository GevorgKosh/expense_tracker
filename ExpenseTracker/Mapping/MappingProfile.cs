using AutoMapper;
using ExpenseTracker.Dto;
using ExpenseTracker.Dto.category;

namespace ExpenseTracker.Mapping;

public class MappingProfile: Profile
{
    public  MappingProfile()
    {
        CreateMap<Expense, ExpenseResponse>();
        CreateMap<ExpenseRequest, Expense>();
        CreateMap<Category, CategoryResponse>();
        CreateMap<CategoryRequest, Category>();
    }
}