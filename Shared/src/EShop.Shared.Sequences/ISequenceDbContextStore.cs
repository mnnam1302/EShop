using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.Sequences;

public interface ISequenceDbContextStore
{
    DbSet<Sequence> Sequences { get; }
}