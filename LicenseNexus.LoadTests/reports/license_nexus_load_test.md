> test info



test suite: `nbomber_default_test_suite_name`

test name: `nbomber_default_test_name`

session id: `2026-02-28_19-29-56_8083f86f`

> scenario stats



scenario: `mixed_scenario`

  - ok count: `233870`

  - fail count: `0`

  - all data: `570.222` MB

  - duration: `00:02:30`

load simulations:

  - `ramping_constant`, copies: `100`, during: `00:00:30`

  - `keep_constant`, copies: `100`, during: `00:02:00`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `233870`, ok = `233870`, RPS = `1559.13`|
|latency (ms)|min = `1.82`, mean = `57.37`, max = `414.49`, StdDev = `46.8`|
|latency percentile (ms)|p50 = `39.74`, p75 = `57.57`, p95 = `161.15`, p99 = `209.41`|
|data transfer (KB)|min = `0.346`, mean = `2.496`, max = `80.268`, all = `570.222` MB|


> status codes for scenario: `mixed_scenario`



|status code|count|message|
|---|---|---|
|NoContent|46479||
|OK|187391||


