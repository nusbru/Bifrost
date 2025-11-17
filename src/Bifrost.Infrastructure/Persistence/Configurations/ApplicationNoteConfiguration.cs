using Bifrost.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bifrost.Infrastructure.Persistence.Configurations;

public class ApplicationNoteConfiguration : EntityConfiguration<ApplicationNote>
{
    public override void Configure(EntityTypeBuilder<ApplicationNote> builder)
    {
        base.Configure(builder);
        builder.Property(an => an.Note).IsRequired();
        builder.Property(an => an.JobApplicationId).IsRequired();
    }
}
