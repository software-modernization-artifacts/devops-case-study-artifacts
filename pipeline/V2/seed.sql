BEGIN;

-- Isola execucao concorrente deste seed na mesma transacao.
SELECT pg_advisory_xact_lock(hashtext('blazorpost_seed_identity_v1'));

-- Limpa entradas seed invalidas de execucoes anteriores (overflow ## / ###).
DELETE FROM blazorserverapp."AspNetUserClaims"
WHERE "UserId" IN (
    SELECT "Id"
    FROM blazorserverapp."AspNetUsers"
    WHERE "Id" LIKE 'seed-user-%#%'
);

DELETE FROM blazorserverapp."AspNetUserRoles"
WHERE "UserId" IN (
    SELECT "Id"
    FROM blazorserverapp."AspNetUsers"
    WHERE "Id" LIKE 'seed-user-%#%'
)
OR "RoleId" IN (
    SELECT "Id"
    FROM blazorserverapp."AspNetRoles"
    WHERE "Id" LIKE 'seed-role-%#%'
);

DELETE FROM blazorserverapp."AspNetUsers"
WHERE "Id" LIKE 'seed-user-%#%';

DELETE FROM blazorserverapp."AspNetRoles"
WHERE "Id" LIKE 'seed-role-%#%';

-- 1000 perfis (1 Administrador + 999 perfis padrao)
WITH role_data AS (
    SELECT
        idx,
        CASE
            WHEN idx = 1 THEN 'seed-role-admin'
            ELSE 'seed-role-' || (
                CASE
                    WHEN (idx - 1) <= 99 THEN lpad((idx - 1)::text, 2, '0')
                    ELSE (idx - 1)::text
                END
            )
        END AS role_id,
        CASE
            WHEN idx = 1 THEN 'Administrador'
            ELSE 'Perfil ' || (
                CASE
                    WHEN (idx - 1) <= 99 THEN lpad((idx - 1)::text, 2, '0')
                    ELSE (idx - 1)::text
                END
            )
        END AS role_name
    FROM generate_series(1, 1000) AS idx
)
INSERT INTO blazorserverapp."AspNetRoles" (
    "Id",
    "Name",
    "NormalizedName",
    "ConcurrencyStamp"
)
SELECT
    role_id,
    role_name,
    upper(role_name),
    md5('seed-role:' || role_id)
FROM role_data
ON CONFLICT DO NOTHING;

-- 10000 usuarios seed
WITH user_data AS (
    SELECT
        idx,
        'seed-user-' || (
            CASE
                WHEN idx <= 999 THEN lpad(idx::text, 3, '0')
                ELSE idx::text
            END
        ) AS user_id,
        'usuario' || (
            CASE
                WHEN idx <= 999 THEN lpad(idx::text, 3, '0')
                ELSE idx::text
            END
        ) AS user_name,
        ('usuario' || (
            CASE
                WHEN idx <= 999 THEN lpad(idx::text, 3, '0')
                ELSE idx::text
            END
        ) || '@seed.local') AS email,
        ('Usuario Seed ' || (
            CASE
                WHEN idx <= 999 THEN lpad(idx::text, 3, '0')
                ELSE idx::text
            END
        )) AS display_name,
        (350000 + idx)::text AS matricula,
        ('Unidade ' || to_char(((idx - 1) % 100) + 1, 'FM00')) AS location_name
    FROM generate_series(1, 10000) AS idx
)
INSERT INTO blazorserverapp."AspNetUsers" (
    "Id",
    "UserName",
    "NormalizedUserName",
    "Email",
    "NormalizedEmail",
    "EmailConfirmed",
    "PasswordHash",
    "SecurityStamp",
    "ConcurrencyStamp",
    "PhoneNumber",
    "PhoneNumberConfirmed",
    "TwoFactorEnabled",
    "LockoutEnd",
    "LockoutEnabled",
    "AccessFailedCount"
)
SELECT
    user_id,
    user_name,
    upper(user_name),
    email,
    upper(email),
    true,
    NULL,
    md5('seed-security:' || user_id),
    md5('seed-concurrency:' || user_id),
    NULL,
    false,
    false,
    NULL,
    true,
    0
FROM user_data
ON CONFLICT DO NOTHING;

-- Claims basicas para os usuarios seed
WITH user_data AS (
    SELECT
        idx,
        'usuario' || (
            CASE
                WHEN idx <= 999 THEN lpad(idx::text, 3, '0')
                ELSE idx::text
            END
        ) AS user_name,
        ('Usuario Seed ' || (
            CASE
                WHEN idx <= 999 THEN lpad(idx::text, 3, '0')
                ELSE idx::text
            END
        )) AS display_name,
        (350000 + idx)::text AS matricula,
        ('Unidade ' || to_char(((idx - 1) % 100) + 1, 'FM00')) AS location_name
    FROM generate_series(1, 10000) AS idx
),
user_target AS (
    SELECT
        au."Id" AS user_id,
        ud.display_name,
        ud.matricula,
        ud.location_name
    FROM user_data ud
    INNER JOIN blazorserverapp."AspNetUsers" au
        ON au."NormalizedUserName" = upper(ud.user_name)
),
claim_data AS (
    SELECT
        ut.user_id,
        c.claim_type,
        c.claim_value
    FROM user_target ut
    CROSS JOIN LATERAL (
        VALUES
            ('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name', ut.display_name),
            ('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname', ut.display_name),
            ('matricula', ut.matricula),
            ('location', ut.location_name),
            ('lotacaoId', '0'),
            ('department', 'TI'),
            ('title', 'Analista')
    ) AS c(claim_type, claim_value)
)
INSERT INTO blazorserverapp."AspNetUserClaims" (
    "UserId",
    "ClaimType",
    "ClaimValue"
)
SELECT
    c.user_id,
    c.claim_type,
    c.claim_value
FROM claim_data c
WHERE NOT EXISTS (
    SELECT 1
    FROM blazorserverapp."AspNetUserClaims" uc
    WHERE uc."UserId" = c.user_id
      AND uc."ClaimType" = c.claim_type
);

-- Vincula usuario001 ao Administrador e os demais distribuidos em Perfil 01..999
WITH user_role_data AS (
    SELECT
        idx,
        'usuario' || (
            CASE
                WHEN idx <= 999 THEN lpad(idx::text, 3, '0')
                ELSE idx::text
            END
        ) AS user_name,
        CASE
            WHEN idx = 1 THEN 'Administrador'
            ELSE 'Perfil ' || (
                CASE
                    WHEN (((idx - 2) % 999) + 1) <= 99 THEN lpad((((idx - 2) % 999) + 1)::text, 2, '0')
                    ELSE (((idx - 2) % 999) + 1)::text
                END
            )
        END AS role_name
    FROM generate_series(1, 10000) AS idx
),
resolved_user_role_data AS (
    SELECT
        au."Id" AS user_id,
        ar."Id" AS role_id
    FROM user_role_data urd
    INNER JOIN blazorserverapp."AspNetUsers" au
        ON au."NormalizedUserName" = upper(urd.user_name)
    INNER JOIN blazorserverapp."AspNetRoles" ar
        ON ar."NormalizedName" = upper(urd.role_name)
)
INSERT INTO blazorserverapp."AspNetUserRoles" (
    "UserId",
    "RoleId"
)
SELECT
    user_id,
    role_id
FROM resolved_user_role_data
ON CONFLICT DO NOTHING;

COMMIT;
