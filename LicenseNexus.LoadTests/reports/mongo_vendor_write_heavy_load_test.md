> test info



test suite: `nbomber_default_test_suite_name`

test name: `nbomber_default_test_name`

session id: `2026-03-03_22-25-29_c1ddc2d5`

> scenario stats



scenario: `write_heavy_scenario`

  - ok count: `189775`

  - fail count: `0`

  - all data: `133.755` MB

  - duration: `00:02:30`

load simulations:

  - `ramping_constant`, copies: `100`, during: `00:00:30`

  - `keep_constant`, copies: `100`, during: `00:02:00`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `189775`, ok = `189775`, RPS = `1265.17`|
|latency (ms)|min = `16.14`, mean = `70.69`, max = `359.24`, StdDev = `25.98`|
|latency percentile (ms)|p50 = `65.15`, p75 = `81.02`, p95 = `119.23`, p99 = `164.1`|
|data transfer (KB)|min = `0.679`, mean = `0.722`, max = `0.787`, all = `133.755` MB|


> status codes for scenario: `write_heavy_scenario`



|status code|count|message|
|---|---|---|
|NoContent|189775||


