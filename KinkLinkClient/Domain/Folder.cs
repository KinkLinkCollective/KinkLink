using System.Collections.Generic;

namespace KinkLinkClient.Domain;

public record Folder<T>(string Path, List<T> Content);
