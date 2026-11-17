using LogHub;

var configPath = args.Length > 0 ? args[0] : "config.txt";
var module = new LogProcessingModule(configPath);
module.Run();
