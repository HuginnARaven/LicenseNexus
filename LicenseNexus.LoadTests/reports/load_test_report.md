> test info



test suite: `nbomber_default_test_suite_name`

test name: `nbomber_default_test_name`

session id: `2026-03-12_23-36-55_b4718d41`

> scenario stats



scenario: `read_heavy_scenario`

  - ok count: `919275`

  - fail count: `0`

  - all data: `342.079` MB

  - duration: `00:02:30`

load simulations:

  - `ramping_constant`, copies: `100`, during: `00:00:30`

  - `keep_constant`, copies: `100`, during: `00:02:00`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `919275`, ok = `919275`, RPS = `6128.5`|
|latency (ms)|min = `1`, mean = `14.59`, max = `121.34`, StdDev = `9.15`|
|latency percentile (ms)|p50 = `14.13`, p75 = `18.94`, p95 = `27.15`, p99 = `56.1`|
|data transfer (KB)|min = `0.348`, mean = `0.381`, max = `0.416`, all = `342.079` MB|


> status codes for scenario: `read_heavy_scenario`



|status code|count|message|
|---|---|---|
|OK|919275||


