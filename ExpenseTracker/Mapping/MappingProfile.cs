using AutoMapper;
using ExpenseTracker.Dto.category;
using ExpenseTracker.Dto.expense;
using ExpenseTracker.Entity;

namespace ExpenseTracker.Mapping;

public class MappingProfile: Profile
{
    public  MappingProfile()
    {
        CreateMap<Expense, ExpenseResponse>()
            .ForMember(d => d.ExpenseDate, o => o.MapFrom(s => DateOnly.FromDateTime(s.ExpenseDate)))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedDateTime));
        CreateMap<ExpenseRequest, Expense>();
        CreateMap<Category, CategoryResponse>();
        CreateMap<CategoryRequest, Category>();
    }
}