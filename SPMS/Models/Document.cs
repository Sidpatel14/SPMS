using System;
using System.Collections.Generic;

namespace SPMS.Models;

public partial class Document
{
    public long DocumentId { get; set; }

    public long ApplicationId { get; set; }

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public string? DocumentType { get; set; }

    public virtual Application Application { get; set; } = null!;
}
