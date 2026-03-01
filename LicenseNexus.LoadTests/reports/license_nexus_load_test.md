> test info



test suite: `nbomber_default_test_suite_name`

test name: `nbomber_default_test_name`

session id: `2026-03-01_12-15-11_522d85a6`

> scenario stats



scenario: `write_heavy_scenario`

  - ok count: `55666`

  - fail count: `0`

  - all data: `39.021` MB

  - duration: `00:02:30`

load simulations:

  - `ramping_constant`, copies: `100`, during: `00:00:30`

  - `keep_constant`, copies: `100`, during: `00:02:00`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `55666`, ok = `55666`, RPS = `371.11`|
|latency (ms)|min = `26.31`, mean = `241.08`, max = `583.64`, StdDev = `62.3`|
|latency percentile (ms)|p50 = `244.74`, p75 = `276.99`, p95 = `335.87`, p99 = `389.38`|
|data transfer (KB)|min = `0.674`, mean = `0.718`, max = `0.773`, all = `39.021` MB|


> status codes for scenario: `write_heavy_scenario`



|status code|count|message|
|---|---|---|
|NoContent|55666||


