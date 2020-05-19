create table if not exists auth
(
    email text primary key,
    password_hash bytea not null,
    user_id uuid not null
);