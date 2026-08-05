import { startAsPromise, awaitPromise } from "./fable_modules/fable-library-js.5.11.0/Async.js";
import { singleton } from "./fable_modules/fable-library-js.5.11.0/AsyncBuilder.js";
import { ofArray } from "./fable_modules/fable-library-js.5.11.0/List.js";
import { Json } from "./fable_modules/Thoth.Json.Core.0.9.0/Types.fs.js";
import { Execute as PortableExecute } from "./Common/Execution/Execution.js";
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
} from "./Common/Results/Results.js";

export {
  ValidationPackageSummary,
  ValidationSummary
} from "./Common/Results/ValidationSummary.js";

export { ARCValidationPackage } from "./Common/Package/ARCValidationPackage.js";
export { PackageArguments } from "./Common/Arguments/PackageArguments.js";
export { Setup } from "./Common/Package/Setup.js";
export * as Badge from "./Common/Outputs/Badge.js";
export * as JUnit from "./Common/Outputs/JUnit.js";

function toJson(value) {
  if (value instanceof Json) {
    return value;
  }
  if (value === null) {
    return Json.Null;
  }
  if (typeof value === "string") {
    return new Json(0, [value]);
  }
  if (typeof value === "number") {
    if (!Number.isFinite(value)) {
      throw new TypeError("ARCExpect payload numbers must be finite.");
    }
    return new Json(1, [value]);
  }
  if (typeof value === "boolean") {
    return new Json(3, [value]);
  }
  if (Array.isArray(value)) {
    return new Json(5, [ofArray(value.map(toJson))]);
  }
  if (
    typeof value === "object"
    && (
      Object.getPrototypeOf(value) === Object.prototype
      || Object.getPrototypeOf(value) === null
    )
  ) {
    const fields = Object.entries(value).map(
      ([key, item]) => [key, toJson(item)]
    );
    return new Json(4, [ofArray(fields)]);
  }
  throw new TypeError(
    "ARCExpect payload values must contain only objects, arrays, "
    + "strings, finite numbers, booleans, or null."
  );
}

export class Execute {
  static validation(validationPackage, options = {}) {
    const {
      payload,
      sourceBranch,
      sourceCommitHash,
      arguments: packageArguments
    } = options;

    return Execute.Validation(
      validationPackage,
      payload,
      sourceBranch,
      sourceCommitHash,
      packageArguments
    );
  }

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
        payload === undefined ? undefined : toJson(payload),
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
