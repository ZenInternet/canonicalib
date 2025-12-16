# Package Comparison - Gap Analysis Report

**Package 1:** Zen.Contract.Orders 0.9.14.1  
**Package 2:** Zen.Contract.Orders 0.9.15-rc

## Summary

| Metric | Count |
|--------|-------|
| Total Types | 20 |
| Identical Types | 0 |
| Modified Types | 4 |
| Namespace Changed | 0 |
| Only in Package 1 | 8 |
| Only in Package 2 | 8 |

## Assemblies

### Package 1 (1 assemblies)

- **Zen.Contract.Orders** v0.9.14.1 (12 public types)

### Package 2 (1 assemblies)

- **Zen.Contract.Orders** v0.9.15.0 (12 public types)

## ⚠️ Types Removed

These types are present in Package 1 but missing in Package 2:

- `Zen.Contract.Orders.CommercialOrderSource` (Enum)
- `Zen.Contract.Orders.FulfilmentAction` (Enum)
- `Zen.Contract.Orders.FulfilmentIntent` (Enum)
- `Zen.Contract.Orders.FulfilmentOrderStatus` (Enum)
- `Zen.Contract.Orders.FulfilmentRequestStatus` (Enum)
- `Zen.Contract.Orders.FulfilmentRequestType` (Enum)
- `Zen.Contract.Orders.FulfilmentStatus` (Enum)
- `Zen.Contract.Orders.FulfilmentSupplementalAction` (Enum)

## ✅ Types Added

These types are new in Package 2:

- `Zen.Contract.Orders.Enums.CommercialOrderSource` (Enum)
- `Zen.Contract.Orders.Enums.FulfilmentAction` (Enum)
- `Zen.Contract.Orders.Enums.FulfilmentIntent` (Enum)
- `Zen.Contract.Orders.Enums.FulfilmentOrderStatus` (Enum)
- `Zen.Contract.Orders.Enums.FulfilmentRequestStatus` (Enum)
- `Zen.Contract.Orders.Enums.FulfilmentRequestType` (Enum)
- `Zen.Contract.Orders.Enums.FulfilmentStatus` (Enum)
- `Zen.Contract.Orders.Enums.FulfilmentSupplementalAction` (Enum)

## ⚡ Types Modified

### `Zen.Contract.Orders.CommercialOrderItemSummary`

- ➖ **Removed**: `.ctor`
  ```
  CommercialOrderItemSummary(String id, String name)
  ```
- ➕ **Added**: `.ctor`
  ```
  CommercialOrderItemSummary()
  ```

### `Zen.Contract.Orders.DecompositionMessage`

- ➖ **Removed**: `.ctor`
  ```
  DecompositionMessage(String text)
  ```
- ➕ **Added**: `.ctor`
  ```
  DecompositionMessage()
  ```

### `Zen.Contract.Orders.FulfilmentRequestSummary`

- ➖ **Removed**: `.ctor`
  ```
  FulfilmentRequestSummary(Nullable`1 action, Nullable`1 createdDate, Nullable`1 id, MetaData metaData, Nullable`1 status, Nullable`1 type)
  ```
- ➕ **Added**: `.ctor`
  ```
  FulfilmentRequestSummary()
  ```

### `Zen.Contract.Orders.MetaData`

- ➖ **Removed**: `AdditionalProperties`
  ```
  IDictionary`2 AdditionalProperties
  ```

## Conclusion

⚠️ **Breaking changes detected!** Package 2 has removed or modified types.
