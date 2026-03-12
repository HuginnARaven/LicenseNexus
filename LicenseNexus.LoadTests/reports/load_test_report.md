> test info



test suite: `nbomber_default_test_suite_name`

test name: `nbomber_default_test_name`

session id: `2026-03-11_22-49-51_d6c67fa5`

> scenario stats



scenario: `write_heavy_patch`

  - ok count: `123368`

  - fail count: `0`

  - all data: `87.183` MB

  - duration: `00:02:30`

load simulations:

  - `ramping_constant`, copies: `80`, during: `00:00:30`

  - `keep_constant`, copies: `80`, during: `00:02:00`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `123368`, ok = `123368`, RPS = `822.45`|
|latency (ms)|min = `15.71`, mean = `86.99`, max = `336.34`, StdDev = `28.97`|
|latency percentile (ms)|p50 = `81.54`, p75 = `100.99`, p95 = `142.46`, p99 = `179.71`|
|data transfer (KB)|min = `0.680`, mean = `0.724`, max = `0.788`, all = `87.183` MB|


> status codes for scenario: `write_heavy_patch`



|status code|count|message|
|---|---|---|
|NoContent|123368||


> scenario stats



scenario: `write_heavy_post`

  - ok count: `38606`

  - fail count: `0`

  - all data: `0` MB

  - duration: `00:02:30`

load simulations:

  - `ramping_constant`, copies: `20`, during: `00:00:30`

  - `keep_constant`, copies: `20`, during: `00:02:00`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `38606`, ok = `38606`, RPS = `257.37`|
|latency (ms)|min = `13.36`, mean = `69.48`, max = `238.83`, StdDev = `22.21`|
|latency percentile (ms)|p50 = `65.79`, p75 = `80`, p95 = `111.68`, p99 = `140.29`|




