import { startAsPromise, awaitPromise } from "./fable_modules/fable-library-js.5.11.0/Async.js";
import { singleton } from "./fable_modules/fable-library-js.5.11.0/AsyncBuilder.js";
import { ofArray } from "./fable_modules/fable-library-js.5.11.0/List.js";
import { Execute as PortableExecute } from "./Execution.js";
import {
  Test_ftestCase,
  Test_ftestCaseAsync,
  Test_ftestList,
  Test_ptestCase,
  Test_ptestCaseAsync,
  Test_ptestList,
  Test_testCase,
  Test_testCaseAsync,
  Test_testList
} from "./fable_modules/Fable.Pyxpecto.2.0.0/Pyxpecto.fs.js";

export {
  CaseOutcome,
  CaseResult,
  RunSummary,
  ValidationResult
} from "./Results.js";

export {
  ValidationPackageSummary,
  ValidationSummary
} from "./ValidationSummary.js";

export { ARCValidationPackage } from "./ARCValidationPackage.js";
export { PackageArguments } from "./PackageArguments.js";
export { FrontmatterLanguage, Setup } from "./Setup.js";
export * as Badge from "./Badge.js";
export * as JUnit from "./JUnit.js";

export class Execute {
  static Validation(
    validationPackage,
    payload,
    sourceBranch,
    sourceCommitHash,
    packageArguments
  ) {
    return startAsPromise(
      PortableExecute.Validation(
        validationPackage,
        payload,
        sourceBranch,
        sourceCommitHash,
        packageArguments
      )
    );
  }
}

export const testCase = Test_testCase;
export const ptestCase = Test_ptestCase;
export const ftestCase = Test_ftestCase;

export function testCaseAsync(name, body) {
  return Test_testCaseAsync(
    name,
    singleton.Delay(() => awaitPromise(Promise.resolve().then(body)))
  );
}

export function ptestCaseAsync(name, body) {
  return Test_ptestCaseAsync(
    name,
    singleton.Delay(() => awaitPromise(Promise.resolve().then(body)))
  );
}

export function ftestCaseAsync(name, body) {
  return Test_ftestCaseAsync(
    name,
    singleton.Delay(() => awaitPromise(Promise.resolve().then(body)))
  );
}

export function testList(name, tests) {
  return Test_testList(name, ofArray(tests));
}

export function ptestList(name, tests) {
  return Test_ptestList(name, ofArray(tests));
}

export function ftestList(name, tests) {
  return Test_ftestList(name, ofArray(tests));
}
