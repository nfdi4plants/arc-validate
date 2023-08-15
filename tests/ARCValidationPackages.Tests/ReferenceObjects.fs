module ReferenceObjects

open ARCValidationPackages

let testDate = System.DateTimeOffset.ParseExact("2023-08-15 10:00:00 +02:00", "yyyy-MM-dd HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture)

let testPackageIndex = 
    [|
        ValidationPackageIndex.create(
            "arc-validate-packages/test.fsx", testDate
        )
    |]

let testConfig =
    Config.create(
        testPackageIndex,
        testDate,
        Defaults.PACKAGE_CACHE_FOLDER(),
        Defaults.CONFIG_FILE_PATH()
    )