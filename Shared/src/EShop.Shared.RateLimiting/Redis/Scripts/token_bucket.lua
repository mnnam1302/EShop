-- KEYS[1..N]: token bucket hash keys (N = 1 or 2)
-- ARGV[3*(i-1)+1] = capacity for KEYS[i]
-- ARGV[3*(i-1)+2] = refill tokens per period for KEYS[i]
-- ARGV[3*(i-1)+3] = refill period in seconds for KEYS[i]
-- Reply: flat array of (allowed, remaining, retry_after_ms) per KEYS[i], in order
-- All-or-nothing: a token is consumed from every key only if every key has one available

local num_keys = #KEYS
local time_result = redis.call('TIME')
local now_seconds = tonumber(time_result[1]) + (tonumber(time_result[2]) / 1000000)

local tokens_after_refill = {}
local retry_after_ms = {}
local would_allow = {}
local all_allowed = true

for i = 1, num_keys do
    local key = KEYS[i]
    local capacity = tonumber(ARGV[3 * (i - 1) + 1])
    local refill_tokens = tonumber(ARGV[3 * (i - 1) + 2])
    local refill_period = tonumber(ARGV[3 * (i - 1) + 3])
    local refill_rate = refill_tokens / refill_period

    local bucket = redis.call('HMGET', key, 'tokens', 'last_refill')
    local tokens = tonumber(bucket[1])
    local last_refill = tonumber(bucket[2])

    if tokens == nil or last_refill == nil then
        tokens = capacity
        last_refill = now_seconds
    end

    local elapsed = math.max(0, now_seconds - last_refill)
    tokens = math.min(capacity, tokens + (elapsed * refill_rate))

    tokens_after_refill[i] = tokens
    would_allow[i] = tokens >= 1

    if would_allow[i] then
        retry_after_ms[i] = 0
    else
        retry_after_ms[i] = math.ceil(((1 - tokens) / refill_rate) * 1000)
        all_allowed = false
    end
end

local reply = {}

for i = 1, num_keys do
    local key = KEYS[i]
    local capacity = tonumber(ARGV[3 * (i - 1) + 1])
    local refill_tokens = tonumber(ARGV[3 * (i - 1) + 2])
    local refill_period = tonumber(ARGV[3 * (i - 1) + 3])
    local refill_rate = refill_tokens / refill_period

    local final_tokens = tokens_after_refill[i]
    if all_allowed then
        final_tokens = final_tokens - 1
    end
    final_tokens = math.max(0, final_tokens)

    local ttl_seconds = math.ceil((capacity / refill_rate) * 2)
    redis.call('HMSET', key, 'tokens', final_tokens, 'last_refill', now_seconds)
    redis.call('EXPIRE', key, ttl_seconds)

    local allowed_flag = 0
    if would_allow[i] then
        allowed_flag = 1
    end

    table.insert(reply, allowed_flag)
    table.insert(reply, math.floor(final_tokens))
    table.insert(reply, retry_after_ms[i])
end

return reply
