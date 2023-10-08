// See https://aka.ms/new-console-template for more information
using Sharpmaid.Service;

if (args.Length < 2) throw new ArgumentException("Please supply arguments.");

var entitiesPath = args[0];
var savePath = args[1];

ClassService classService = new ClassService();

classService.ReadFiles(entitiesPath);

classService.WriteUmlText(savePath);
