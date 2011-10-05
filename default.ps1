properties {
    $buildDir = ".\build"
    $outputDir = $buildDir + "\lib\" + $framework
    $out = $outputDir + "\xizzle.dll"
}

task default -depends Compile, Clean

task Init -depends Clean {
    mkdir $outputDir | out-null
}

task Compile -depends Init { 
    $sources = gci ".\xizzle" -r -fi *.cs |% { $_.FullName }
    csc /target:library /out:$out $sources
}

task Test -depends Compile {
    . $nunit $out
}

task Clean { 
    if (test-path $outputDir) { 
        ri -r -fo $outputDir 
    }
}
