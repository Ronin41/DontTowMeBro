param($installPath, $toolsPath, $package, $project)

$item = $project.ProjectItems | where-object {$_.Name -eq "defaultAdSettings.xml"}

$item.Properties.Item("BuildAction").Value = [int]9

$item2 = $project.ProjectItems | where-object {$_.Name -eq "ReadMe-AdRotator-XAML.txt"}

$item2.Properties.Item("BuildAction").Value = [int]0